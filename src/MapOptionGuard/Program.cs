namespace MapOptionGuard;

internal static class Program
{
    public const string AppTitle = "맵 옵션 가드";
    public const string OcrLanguage = "ko";
    private const string MutexName = "MapOptionGuard.SingleInstance";

    [STAThread]
    private static void Main(string[] args)
    {
        // DPI 인식을 먼저 켜야 진단 모드도 실제 픽셀 좌표로 읽는다
        ApplicationConfiguration.Initialize();

        if (args.Length > 0 && args[0] == SelfTest.Argument)
        {
            // 창을 띄우지 않으므로 실행 중인 프로그램과 함께 돌려도 되게 중복 실행 검사는 건너뛴다
            Environment.ExitCode = SelfTest.RunAsync(args[1..]).GetAwaiter().GetResult();
            return;
        }

        using var mutex = new Mutex(initiallyOwned: true, MutexName, out bool isFirstInstance);
        if (!isFirstInstance)
        {
            MessageBox.Show("이미 실행 중입니다. 작업 표시줄에서 맵 옵션 가드 창을 확인하세요.", AppTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        AppConfig config;
        try
        {
            config = AppConfig.LoadOrCreate();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"설정 파일을 읽지 못했습니다.\n{AppConfig.FilePath}\n\n{ex.Message}", AppTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        OptionCollector collector;
        try
        {
            collector = OptionCollector.Load();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"옵션 수집 파일을 읽지 못했습니다.\n{OptionCollector.FilePath}\n\n{ex.Message}", AppTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (collector.BackupPath is { } backupPath)
        {
            MessageBox.Show($"옵션 수집 파일이 깨져 있어 새로 시작합니다.\n기존 파일은 여기에 보관했습니다.\n{backupPath}",
                AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        var scanner = OptionScanner.TryCreate(OcrLanguage);
        if (scanner is null)
        {
            MessageBox.Show(
                "Windows 한국어 OCR을 찾지 못했습니다.\n" +
                "설정 > 시간 및 언어 > 언어 및 지역에서 한국어를 추가하고 '광학 문자 인식'을 설치한 뒤 다시 실행하세요.",
                AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.Run(new MainForm(config, scanner, collector));
    }
}
