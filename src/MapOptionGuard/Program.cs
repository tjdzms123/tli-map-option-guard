namespace MapOptionGuard;

internal static class Program
{
    public const string AppTitle = "맵 옵션 가드";
    private const string OcrLanguage = "ko";
    private const string MutexName = "MapOptionGuard.SingleInstance";

    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var mutex = new Mutex(initiallyOwned: true, MutexName, out bool isFirstInstance);
        if (!isFirstInstance)
        {
            MessageBox.Show("이미 실행 중입니다. 작업 표시줄의 트레이 아이콘을 확인하세요.", AppTitle,
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

        var scanner = OptionScanner.TryCreate(OcrLanguage);
        if (scanner is null)
        {
            MessageBox.Show(
                "Windows 한국어 OCR을 찾지 못했습니다.\n" +
                "설정 > 시간 및 언어 > 언어 및 지역에서 한국어를 추가하고 '광학 문자 인식'을 설치한 뒤 다시 실행하세요.",
                AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.Run(new GuardContext(config, scanner));
    }
}
