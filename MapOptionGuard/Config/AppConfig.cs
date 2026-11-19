namespace MapOptionGuard;

/// <summary>
/// 게임 창 클라이언트 영역 기준의 비율 좌표(0~1). 창 크기가 바뀌어도 같은 UI 위치를 가리킨다.
/// </summary>
internal sealed class RelativeRect
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public Rectangle ToScreen(Rectangle client) => Rectangle.FromLTRB(
        client.X + (int)Math.Round(X * client.Width),
        client.Y + (int)Math.Round(Y * client.Height),
        client.X + (int)Math.Round((X + Width) * client.Width),
        client.Y + (int)Math.Round((Y + Height) * client.Height));

    public static RelativeRect FromClient(Rectangle rect, Size clientSize) => new()
    {
        X = Math.Round((double)rect.X / clientSize.Width, 4),
        Y = Math.Round((double)rect.Y / clientSize.Height, 4),
        Width = Math.Round((double)rect.Width / clientSize.Width, 4),
        Height = Math.Round((double)rect.Height / clientSize.Height, 4),
    };
}

internal sealed class AppConfig
{
    public string WindowTitle { get; set; } = "Torchlight: Infinite";
    // 같은 제목을 쓰는 도구 창(TTD 등)과 구분하기 위한 게임 프로세스 이름(.exe 제외). 비우면 확인하지 않는다.
    public string ProcessName { get; set; } = "torchlight_infinite";
    public int IntervalMs { get; set; } = 500;
    public double OcrScale { get; set; } = 2.0;
    public bool FuzzyMatch { get; set; } = true;
    public string PanelAnchor { get; set; } = "기본 옵션";
    public RelativeRect OptionRegion { get; set; } = DefaultOptionRegion();
    public RelativeRect ResetButtonRegion { get; set; } = DefaultResetButtonRegion();
    // 거를 옵션이 없을 때 초록 네모를 띄울 '포탈 개방' 버튼 영역
    public RelativeRect PortalButtonRegion { get; set; } = DefaultPortalButtonRegion();
    public List<string> BlockedKeywords { get; set; } = ["실명", "마비"];
    // 맵 보상(드롭 수량 등)이라 거를 옵션 후보로 모으지 않을 키워드
    public List<string> RewardKeywords { get; set; } = ["드롭 수량"];
    // 띄어쓰기를 뺀 글자가 이 비율 이상 같으면 같은 옵션으로 본다(0.5~1.0). 모으기·스크롤 기억·거르기 판정에 쓴다.
    public double SameOptionSimilarity { get; set; } = 0.8;
    public string BoxColor { get; set; } = "#FF2020";
    // 포탈 개방 버튼에 띄우는 '거를 옵션 없음' 네모 색
    public string OkBoxColor { get; set; } = "#20C040";
    public int BoxThickness { get; set; } = 4;
    public bool CollectOptions { get; set; } = true;

    public static string FilePath { get; } = Path.Combine(AppContext.BaseDirectory, "config.json");

    /// <summary>설정 파일을 읽는다. 파일이 없으면 기본값으로 새로 만들고, 형식 오류는 예외로 올린다.</summary>
    public static AppConfig LoadOrCreate()
    {
        if (!File.Exists(FilePath))
        {
            var created = new AppConfig();
            created.Save();
            return created;
        }

        var config = JsonFileStore.Load<AppConfig>(FilePath)
                     ?? throw new InvalidDataException("설정 파일이 비어 있습니다.");
        config.Normalize();
        return config;
    }

    public void Save() => JsonFileStore.Save(FilePath, this);

    public Color GetBoxColor() => ParseColor(BoxColor, Color.Red);

    public Color GetOkBoxColor() => ParseColor(OkBoxColor, Color.LimeGreen);

    // 색 이름이나 #RRGGBB를 읽고, 잘못 적었으면 기본 색을 쓴다
    private static Color ParseColor(string text, Color fallback)
    {
        try
        {
            return ColorTranslator.FromHtml(text);
        }
        catch (Exception)
        {
            return fallback;
        }
    }

    // 사용자가 값을 비우거나 범위를 벗어나게 적어도 동작하도록 보정한다
    private void Normalize()
    {
        WindowTitle ??= "Torchlight: Infinite";
        ProcessName = NormalizeProcessName(ProcessName);
        PanelAnchor ??= "";
        OptionRegion ??= DefaultOptionRegion();
        ResetButtonRegion ??= DefaultResetButtonRegion();
        PortalButtonRegion ??= DefaultPortalButtonRegion();
        BlockedKeywords = RemoveEmptyKeywords(BlockedKeywords);
        RewardKeywords = RemoveEmptyKeywords(RewardKeywords);
        BoxColor ??= "#FF2020";
        OkBoxColor ??= "#20C040";
        IntervalMs = Math.Clamp(IntervalMs, 200, 5000);
        OcrScale = Math.Clamp(OcrScale, 1.0, 4.0);
        SameOptionSimilarity = Math.Clamp(SameOptionSimilarity, 0.5, 1.0);
        BoxThickness = Math.Clamp(BoxThickness, 1, 20);
    }

    private static List<string> RemoveEmptyKeywords(List<string>? keywords) =>
        keywords?.Where(k => !string.IsNullOrWhiteSpace(k)).ToList() ?? [];

    /// <summary>앞뒤 공백과 끝의 .exe를 떼어 Process.ProcessName과 비교할 수 있는 형태로 만든다.</summary>
    public static string NormalizeProcessName(string? processName)
    {
        var name = processName?.Trim() ?? "";
        return name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? name[..^4] : name;
    }

    // 기본 좌표는 1920x1010 창 모드 스크린샷 기준 대략값이다. 메인 창의 '영역 설정'으로 맞추는 것을 권장한다.
    private static RelativeRect DefaultOptionRegion() => new() { X = 0.038, Y = 0.270, Width = 0.172, Height = 0.418 };

    private static RelativeRect DefaultResetButtonRegion() => new() { X = 0.042, Y = 0.714, Width = 0.079, Height = 0.034 };

    // 맵 화면 오른쪽 아래 '포탈 개방' 버튼 대략값
    private static RelativeRect DefaultPortalButtonRegion() => new() { X = 0.885, Y = 0.909, Width = 0.094, Height = 0.043 };
}
