using System.Text;

namespace MapOptionGuard;

/// <summary>
/// 진단 모드. 창을 띄우지 않고 옵션 영역을 한 번 읽어 창 찾기·OCR·옵션 나누기·판정 결과를 텍스트 파일로 남긴다.
/// <code>
/// MapOptionGuard.exe --selftest [보고서 경로]                 지금 게임 화면을 캡처해서 읽는다(포커스 무관)
/// MapOptionGuard.exe --selftest [보고서 경로] --image 캡처.png  저장해 둔 옵션 영역 캡처를 다시 읽는다
/// </code>
/// 화면을 읽을 때는 원본 캡처를 selftest_raw.png로 남겨 두어, 나중에 --image로 같은 화면을 다시 시험할 수 있다.
/// </summary>
internal static class SelfTest
{
    public const string Argument = "--selftest";
    private const string ImageOption = "--image";

    public static async Task<int> RunAsync(IReadOnlyList<string> args)
    {
        int imageIndex = args.ToList().IndexOf(ImageOption);
        string? imagePath = imageIndex >= 0 && imageIndex + 1 < args.Count ? args[imageIndex + 1] : null;
        string reportPath = args.Count > 0 && args[0] != ImageOption
            ? args[0]
            : Path.Combine(AppContext.BaseDirectory, "selftest.txt");
        string outputDir = Path.GetDirectoryName(Path.GetFullPath(reportPath))!;

        var report = new StringBuilder();
        report.AppendLine($"[진단] {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        try
        {
            var config = AppConfig.LoadOrCreate();
            report.AppendLine($"설정 파일: {AppConfig.FilePath}");

            using var scanner = OptionScanner.TryCreate(Program.OcrLanguage)
                                ?? throw new InvalidOperationException("Windows 한국어 OCR이 없습니다.");
            using var guard = new OptionGuard(config, scanner, OptionCollector.Load());

            IReadOnlyList<string> lines;
            string processName;
            Rectangle client;
            if (imagePath is not null)
            {
                report.AppendLine($"입력 이미지: {imagePath}");
                using var image = new Bitmap(imagePath);
                lines = await guard.ReadLinesAsync(image);
                processName = "(이미지)";
                client = Rectangle.Empty;
            }
            else
            {
                report.AppendLine($"찾는 창: 제목 '{config.WindowTitle}', 프로세스 '{config.ProcessName}'");
                foreach (var candidate in GameWindow.FindCandidates(config.WindowTitle))
                {
                    var rect = candidate.GetClientScreenRect();
                    report.AppendLine($"  후보 창: {candidate.ProcessName} · {rect.Width}×{rect.Height} · 위치 {rect.X},{rect.Y}");
                }

                var game = GameWindow.Find(config.WindowTitle, config.ProcessName, out var failureMessage);
                if (game is null)
                {
                    report.AppendLine("게임 창 없음: " + failureMessage);
                    return 1;
                }

                client = game.GetClientScreenRect();
                processName = game.ProcessName;
                var optionRect = config.OptionRegion.ToScreen(client);
                report.AppendLine($"게임 화면: {client} · 옵션 영역: {optionRect} · 초기화 버튼: {config.ResetButtonRegion.ToScreen(client)}");

                using var raw = ScreenCapture.Capture(optionRect);
                var rawPath = Path.Combine(outputDir, "selftest_raw.png");
                raw.Save(rawPath);
                report.AppendLine($"원본 캡처: {rawPath}");
                lines = await guard.ReadLinesAsync(raw);
            }

            var result = guard.Analyze(lines, processName, client);
            report.AppendLine($"상태: {result.State} · {result.Message}");

            report.AppendLine().AppendLine("[OCR 줄]");
            foreach (var line in lines)
            {
                report.AppendLine("  " + line);
            }

            report.AppendLine().AppendLine("[옵션으로 나눈 결과]");
            foreach (var option in OptionLineParser.SplitOptions(lines, config.PanelAnchor, config.FuzzyMatch))
            {
                report.AppendLine("  - " + option);
            }

            var preparedPath = Path.Combine(outputDir, "selftest_ocr.png");
            if (guard.SaveLastImage(preparedPath))
            {
                report.AppendLine().AppendLine($"OCR 입력 이미지: {preparedPath}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            report.AppendLine("오류: " + ex);
            return 1;
        }
        finally
        {
            File.WriteAllText(reportPath, report.ToString());
        }
    }
}
