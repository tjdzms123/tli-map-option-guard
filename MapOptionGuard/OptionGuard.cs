namespace MapOptionGuard;

internal enum ScanState
{
    GameNotFound,
    GameNotActive,
    InvalidRegion,
    PanelClosed,
    Clean,
    Blocked,
}

/// <summary>검사 한 번의 결과. 화면 표시와 오버레이 판단에 쓴다.</summary>
internal sealed record ScanResult(
    ScanState State,
    string Message,
    string GameProcessName,
    Rectangle GameClient,
    string OcrText,
    IReadOnlyList<string> Blocked,
    Rectangle ResetButtonRect)
{
    /// <summary>OCR까지 실제로 돌았는지. 아니면 OcrText가 비어 있다.</summary>
    public bool RanOcr => State is ScanState.PanelClosed or ScanState.Clean or ScanState.Blocked;

    public static ScanResult Skipped(ScanState state, string message, GameWindow? game = null,
        Rectangle gameClient = default) =>
        new(state, message, game?.ProcessName ?? "", gameClient, "", [], Rectangle.Empty);
}

/// <summary>
/// 게임 창을 찾아 옵션 영역을 읽고, 옵션을 모으고, 거를 옵션이 있는지 판정한다. 화면(UI)은 다루지 않는다.
/// </summary>
internal sealed class OptionGuard : IDisposable
{
    private readonly OptionScanner _scanner;
    private AppConfig _config;

    public OptionGuard(AppConfig config, OptionScanner scanner, OptionCollector collector)
    {
        _scanner = scanner;
        Collector = collector;
        _config = config;
        RemoveCollectedRewardOptions();
    }

    /// <summary>바꾸면 이미 모아 둔 옵션 중 보상 옵션도 거를 옵션 후보에서 뺀다.</summary>
    public AppConfig Config
    {
        get => _config;
        set
        {
            _config = value;
            RemoveCollectedRewardOptions();
        }
    }

    public OptionCollector Collector { get; }

    public async Task<ScanResult> ScanAsync()
    {
        var config = Config;
        var game = GameWindow.Find(config.WindowTitle, config.ProcessName, out var failureMessage);
        if (game is null)
        {
            return ScanResult.Skipped(ScanState.GameNotFound, failureMessage);
        }

        var client = game.GetClientScreenRect();
        if (game.IsMinimized || !game.IsForeground)
        {
            return ScanResult.Skipped(ScanState.GameNotActive, "게임 창이 맨 앞에 있을 때만 검사합니다.", game, client);
        }

        var optionRect = config.OptionRegion.ToScreen(client);
        if (optionRect.Width <= 0 || optionRect.Height <= 0)
        {
            return ScanResult.Skipped(ScanState.InvalidRegion, "옵션 영역 크기가 0입니다. '영역 설정'을 다시 해 주세요.",
                game, client);
        }

        var lines = await _scanner.ReadLinesAsync(optionRect, config.OcrScale);
        return Analyze(lines, game.ProcessName, client);
    }

    /// <summary>
    /// OCR 줄로 옵션 창이 열렸는지 보고, 옵션을 모은 뒤 거를 옵션이 있는지 판정한다.
    /// 진단 모드가 저장해 둔 캡처 이미지를 읽을 때도 같은 판정을 쓴다.
    /// </summary>
    public ScanResult Analyze(IReadOnlyList<string> lines, string processName, Rectangle client)
    {
        var config = Config;
        var ocrText = string.Join("\n", lines);
        var compactText = KeywordMatcher.RemoveWhitespace(ocrText);
        bool panelVisible = string.IsNullOrWhiteSpace(config.PanelAnchor)
                            || KeywordMatcher.Contains(compactText, config.PanelAnchor, config.FuzzyMatch);
        if (!panelVisible)
        {
            return new ScanResult(ScanState.PanelClosed,
                $"'{config.PanelAnchor}' 제목이 보이지 않아 옵션 창이 닫힌 것으로 봤습니다.",
                processName, client, ocrText, [], Rectangle.Empty);
        }

        var collectError = config.CollectOptions ? TryCollect(lines, config) : null;
        var blockRules = config.BlockedKeywords.Concat(Collector.GetBlockedTexts());
        var blocked = KeywordMatcher.FindBlocked(compactText, blockRules, config.FuzzyMatch);

        var message = blocked.Count > 0 ? "걸린 옵션: " + string.Join(", ", blocked) : "걸린 옵션 없음";
        if (collectError is not null)
        {
            message += " / 옵션 수집 실패: " + collectError;
        }

        return new ScanResult(blocked.Count > 0 ? ScanState.Blocked : ScanState.Clean, message,
            processName, client, ocrText, blocked, config.ResetButtonRegion.ToScreen(client));
    }

    /// <summary>이미 가진 이미지를 OCR한다(진단 모드용).</summary>
    public Task<IReadOnlyList<string>> ReadLinesAsync(Bitmap image) => _scanner.ReadLinesAsync(image, Config.OcrScale);

    public bool SaveLastImage(string path) => _scanner.SaveLastImage(path);

    /// <summary>드롭 수량처럼 거를 대상이 아닌 맵 보상 옵션인지.</summary>
    public bool IsRewardOption(string optionText)
    {
        var compact = KeywordMatcher.RemoveWhitespace(optionText);
        return _config.RewardKeywords.Any(keyword => KeywordMatcher.Contains(compact, keyword, _config.FuzzyMatch));
    }

    public void Dispose() => _scanner.Dispose();

    // 보상 키워드를 추가하기 전에 모아 둔 옵션도 목록에서 빠지도록 한다
    private void RemoveCollectedRewardOptions() => Collector.RemoveUnblocked(IsRewardOption);

    // 수집 파일 저장이 실패해도 거르기 판정은 계속되도록 오류는 메시지로만 돌려준다
    private string? TryCollect(IReadOnlyList<string> lines, AppConfig config)
    {
        try
        {
            var options = OptionLineParser.SplitOptions(lines, config.PanelAnchor, config.FuzzyMatch)
                .Where(option => !IsRewardOption(option))
                .ToList();
            Collector.Record(options);
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
