using System.Text.Json;
using System.Text.Json.Serialization;

namespace MapOptionGuard;

/// <summary>한 번이라도 본 맵 기본 옵션. Blocked가 켜진 옵션이 보이면 초기화를 권장한다.</summary>
internal sealed class SeenOption
{
    private string _text = "";
    private string? _compactText;

    /// <summary>바꾸면 띄어쓰기를 뺀 글자(CompactText)도 다시 만든다. 선택 창에서 글자를 고칠 때 바뀐다.</summary>
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            _compactText = null;
        }
    }

    public int Count { get; set; }
    public string FirstSeen { get; set; } = "";
    public string LastSeen { get; set; } = "";
    public bool Blocked { get; set; }

    [JsonIgnore]
    public string CompactText => _compactText ??= KeywordMatcher.RemoveWhitespace(Text);
}

/// <summary>
/// 검사하면서 본 옵션을 모아 collected_options.json에 저장한다.
/// OCR이 같은 옵션을 조금씩 다르게 읽으므로, 설정한 비율 이상 비슷하면 같은 옵션으로 보고 한 항목에 센다.
/// </summary>
internal sealed class OptionCollector
{
    private const string TimeFormat = "yyyy-MM-dd HH:mm";

    private readonly List<SeenOption> _options;

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

    /// <summary>
    /// 거르기로 고르지 않은 옵션 중 조건에 맞는 것을 목록에서 뺀다.
    /// 파일에는 다음 저장 때 반영된다. 이미 거르기로 고른 옵션은 사용자가 정한 것이라 남긴다.
    /// </summary>
    public void RemoveUnblocked(Func<string, bool> shouldRemove) =>
        _options.RemoveAll(o => !o.Blocked && shouldRemove(o.Text));

    /// <summary>사용자가 고른 옵션을 목록에서 뺀다. 거르기로 고른 옵션이어도 뺀다. 파일에는 다음 저장 때 반영된다.</summary>
    public void Remove(IReadOnlyCollection<SeenOption> options) => _options.RemoveAll(o => options.Contains(o));

    public void Save() => JsonFileStore.Save(FilePath, _options);

    /// <summary>
    /// 옵션 창에서 처음 본 옵션들을 기록한다. 같은 옵션 창을 다시 세지 않도록 거르는 일은 PanelOptionMemory가 맡는다.
    /// </summary>
    public void Record(IReadOnlyList<string> newOptions, double minSimilarity)
    {
        if (newOptions.Count == 0)
        {
            return;
        }

        var now = DateTime.Now.ToString(TimeFormat);
        foreach (var text in newOptions.Select(option => option.Trim()))
        {
            var existing = FindSimilar(_options, KeywordMatcher.RemoveWhitespace(text), minSimilarity);
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

    /// <summary>
    /// 비슷한 옵션이 따로 모여 있으면 한 항목으로 합친다. 본 횟수가 많은 쪽 글자를 남기고 횟수는 더한다.
    /// 파일에는 다음 저장 때 반영된다.
    /// </summary>
    public void MergeSimilar(double minSimilarity)
    {
        var merged = new List<SeenOption>();
        foreach (var option in _options.OrderByDescending(o => o.Count))
        {
            var same = FindSimilar(merged, option.CompactText, minSimilarity);
            if (same is null)
            {
                merged.Add(option);
            }
            else
            {
                Absorb(same, option);
            }
        }

        _options.Clear();
        _options.AddRange(merged);
    }

    /// <summary>
    /// target과 비슷한 다른 옵션을 target 하나로 합친다. 선택 창에서 고친 글자가 남도록 target의 글자를 쓴다.
    /// 파일에는 다음 저장 때 반영된다.
    /// </summary>
    /// <returns>합쳐서 없어진 옵션 수</returns>
    public int MergeInto(SeenOption target, double minSimilarity)
    {
        var similar = _options
            .Where(o => o != target && KeywordMatcher.IsSimilar(o.CompactText, target.CompactText, minSimilarity))
            .ToList();
        foreach (var option in similar)
        {
            Absorb(target, option);
        }

        return _options.RemoveAll(o => similar.Contains(o));
    }

    // 같은 옵션으로 본 source의 기록을 target에 더한다. 어느 한쪽이라도 거르기로 골랐으면 거른다.
    private static void Absorb(SeenOption target, SeenOption source)
    {
        target.Count += source.Count;
        target.Blocked |= source.Blocked;
        // 시간은 yyyy-MM-dd HH:mm 형식이라 글자 순서가 곧 시간 순서다
        if (source.FirstSeen.Length > 0 && string.CompareOrdinal(source.FirstSeen, target.FirstSeen) < 0)
        {
            target.FirstSeen = source.FirstSeen;
        }

        if (string.CompareOrdinal(source.LastSeen, target.LastSeen) > 0)
        {
            target.LastSeen = source.LastSeen;
        }
    }

    private static SeenOption? FindSimilar(List<SeenOption> options, string compactText, double minSimilarity) =>
        options.Find(o => KeywordMatcher.IsSimilar(o.CompactText, compactText, minSimilarity));
}
