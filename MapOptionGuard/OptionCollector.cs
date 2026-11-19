using System.Text.Json;
using System.Text.Json.Serialization;

namespace MapOptionGuard;

/// <summary>한 번이라도 본 맵 기본 옵션. Blocked가 켜진 옵션이 보이면 초기화를 권장한다.</summary>
internal sealed class SeenOption
{
    private string? _compactText;

    public string Text { get; set; } = "";
    public int Count { get; set; }
    public string FirstSeen { get; set; } = "";
    public string LastSeen { get; set; } = "";
    public bool Blocked { get; set; }

    [JsonIgnore]
    public string CompactText => _compactText ??= KeywordMatcher.RemoveWhitespace(Text);
}

/// <summary>
/// 검사하면서 본 옵션을 중복 없이 모아 collected_options.json에 저장한다.
/// 같은 옵션 창을 0.5초마다 다시 세지 않도록, 두 번 연속 같게 읽힌 새 옵션 창만 한 번 기록한다.
/// </summary>
internal sealed class OptionCollector
{
    // 숫자나 짧은 OCR 조각은 옵션으로 보지 않는다
    private const int MinOptionLength = 4;
    private const string TimeFormat = "yyyy-MM-dd HH:mm";

    private readonly List<SeenOption> _options;
    private string _previousSignature = "";
    private string _recordedSignature = "";

    private OptionCollector(List<SeenOption> options, string? backupPath)
    {
        _options = options;
        BackupPath = backupPath;
    }

    public static string FilePath { get; } = Path.Combine(AppContext.BaseDirectory, "collected_options.json");

    /// <summary>불러올 때 파일이 깨져 있어 옆으로 옮겨 둔 경우 그 경로.</summary>
    public string? BackupPath { get; }

    public IReadOnlyList<SeenOption> Options => _options;

    public int Count => _options.Count;

    public static OptionCollector Load()
    {
        if (!File.Exists(FilePath))
        {
            return new OptionCollector([], null);
        }

        try
        {
            var options = JsonFileStore.Load<List<SeenOption>>(FilePath) ?? [];
            return new OptionCollector(options.Where(o => !string.IsNullOrWhiteSpace(o.Text)).ToList(), null);
        }
        catch (JsonException)
        {
            // 모아 둔 기록을 잃지 않도록 깨진 파일은 지우지 않고 옆에 보관한 뒤 새로 시작한다
            var backupPath = Path.ChangeExtension(FilePath, $".broken-{DateTime.Now:yyyyMMdd-HHmmss}.json");
            File.Move(FilePath, backupPath);
            return new OptionCollector([], backupPath);
        }
    }

    public IEnumerable<string> GetBlockedTexts() => _options.Where(o => o.Blocked).Select(o => o.Text);

    public void Save() => JsonFileStore.Save(FilePath, _options);

    /// <summary>옵션 창 하나에서 읽은 옵션들을 기록한다.</summary>
    public void Record(IReadOnlyList<string> panelOptions)
    {
        var candidates = panelOptions
            .Select(text => (Text: text.Trim(), Compact: KeywordMatcher.RemoveWhitespace(text)))
            .Where(option => option.Compact.Length >= MinOptionLength)
            .ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        var signature = string.Join("|", candidates.Select(o => o.Compact).Order(StringComparer.Ordinal));
        bool isStable = signature == _previousSignature;
        _previousSignature = signature;
        if (!isStable || signature == _recordedSignature)
        {
            // 화면이 바뀌는 중이거나 이미 기록한 옵션 창
            return;
        }

        _recordedSignature = signature;
        var now = DateTime.Now.ToString(TimeFormat);
        foreach (var (text, compact) in candidates)
        {
            // OCR이 같은 옵션을 조금씩 다르게 읽어도 한 항목으로 모은다
            int allowedMismatch = KeywordMatcher.AllowedMismatch(compact.Length, fuzzy: true);
            var existing = _options.Find(o => KeywordMatcher.AreNearlyEqual(o.CompactText, compact, allowedMismatch));
            if (existing is null)
            {
                _options.Add(new SeenOption { Text = text, Count = 1, FirstSeen = now, LastSeen = now });
            }
            else
            {
                existing.Count++;
                existing.LastSeen = now;
            }
        }

        Save();
    }
}
