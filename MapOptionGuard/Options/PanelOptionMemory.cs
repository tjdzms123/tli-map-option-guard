namespace MapOptionGuard;

/// <summary>
/// 지금 열린 맵의 기본 옵션을 위에서부터 순서대로 기억한다. 옵션이 많으면 창을 내려야 다 보이므로,
/// 스크롤하며 본 옵션을 이어 붙여 두고 거르기 판정에 같이 쓴다.
/// 새로 읽은 화면이 기억한 목록의 한 구간과 겹치면 스크롤로 보고 합치고,
/// 겹치지 않으면(옵션 초기화, 다른 맵) 기억을 새로 시작한다.
/// </summary>
internal sealed class PanelOptionMemory
{
    // 숫자나 짧은 OCR 조각은 옵션으로 보지 않는다
    private const int MinOptionLength = 4;

    // 옵션 초기화 뒤 우연히 옵션 하나가 같은 경우를 스크롤로 착각하지 않도록, 연달아 두 개 이상 겹쳐야 같은 창으로 본다
    private const int MinScrollOverlap = 2;

    private readonly List<PanelOption> _options = [];
    private string _pendingSignature = "";
    private string _appliedSignature = "";

    public int Count => _options.Count;

    /// <summary>기억한 옵션들(띄어쓰기 없음)</summary>
    public IEnumerable<string> CompactOptions => _options.Select(option => option.Compact);

    /// <summary>기억한 옵션을 띄어쓰기 없이 이은 글자. 옵션 사이를 '|'로 나눠 두 옵션에 걸친 글자가 키워드로 걸리지 않게 한다.</summary>
    public string CompactText { get; private set; } = "";

    /// <summary>
    /// 이번 검사에서 보인 옵션들을 반영한다. 스크롤 중인 화면을 피하려고 두 번 연속 같게 읽힌 화면만 쓴다.
    /// </summary>
    /// <param name="minSimilarity">이 비율 이상 비슷하면 같은 옵션으로 본다</param>
    /// <returns>이 옵션 창에서 처음 본 옵션. 창 가장자리에서 잘려 보이던 옵션이 온전히 보이게 된 경우도 포함한다.</returns>
    public IReadOnlyList<string> Update(IEnumerable<string> visibleOptions, double minSimilarity)
    {
        var window = visibleOptions
            .Select(text => new PanelOption(text.Trim()))
            .Where(option => option.Compact.Length >= MinOptionLength)
            .ToList();
        var signature = string.Join("|", window.Select(option => option.Compact));
        bool isStable = signature == _pendingSignature;
        _pendingSignature = signature;
        if (!isStable || window.Count == 0 || signature == _appliedSignature)
        {
            // 화면이 바뀌는 중이거나 이미 반영한 화면
            return [];
        }

        _appliedSignature = signature;
        var added = FindScrollShift(window, minSimilarity) is { } shift ? MergeAt(window, shift) : StartOver(window);
        CompactText = string.Join("|", _options.Select(option => option.Compact));
        return added;
    }

    private List<string> StartOver(List<PanelOption> window)
    {
        _options.Clear();
        _options.AddRange(window);
        return window.Select(option => option.Text).ToList();
    }

    /// <summary>
    /// window[i]가 기억한 목록의 _options[i + shift]에 해당하는 shift를 찾는다.
    /// 겹치는 자리의 옵션이 모두 같아야 하므로, 일부 옵션만 바뀐 경우도 새 옵션 창으로 본다.
    /// </summary>
    private int? FindScrollShift(List<PanelOption> window, double minSimilarity)
    {
        for (int shift = MinScrollOverlap - window.Count; shift <= _options.Count - MinScrollOverlap; shift++)
        {
            int start = Math.Max(0, -shift);
            int end = Math.Min(window.Count, _options.Count - shift);
            if (end - start < MinScrollOverlap)
            {
                continue;
            }

            bool allMatch = true;
            for (int i = start; i < end && allMatch; i++)
            {
                allMatch = _options[i + shift].Matches(window[i], minSimilarity);
            }

            if (allMatch)
            {
                return shift;
            }
        }

        return null;
    }

    private List<string> MergeAt(List<PanelOption> window, int shift)
    {
        var added = new List<string>();
        if (shift < 0)
        {
            // 위로 올려서 새로 보인 옵션을 앞에 붙이면, window[i]가 _options[i]에 해당하게 된다
            var above = window.Take(-shift).ToList();
            _options.InsertRange(0, above);
            added.AddRange(above.Select(option => option.Text));
            shift = 0;
        }

        for (int i = 0; i < window.Count; i++)
        {
            int index = i + shift;
            if (index >= _options.Count)
            {
                // 아래로 내려서 새로 보인 옵션
                _options.Add(window[i]);
                added.Add(window[i].Text);
            }
            else if (window[i].Compact.Length > _options[index].Compact.Length)
            {
                // 창 가장자리에서 잘려 보이던 옵션이 온전히 보이게 됐다
                _options[index] = window[i];
                added.Add(window[i].Text);
            }
        }

        return added;
    }

    private sealed class PanelOption(string text)
    {
        public string Text { get; } = text;

        public string Compact { get; } = KeywordMatcher.RemoveWhitespace(text);

        /// <summary>OCR이 같은 옵션을 조금 다르게 읽었거나, 한쪽이 창 가장자리에서 잘린 조각인지.</summary>
        public bool Matches(PanelOption other, double minSimilarity)
        {
            var (shorter, longer) = Compact.Length <= other.Compact.Length
                ? (Compact, other.Compact)
                : (other.Compact, Compact);
            return KeywordMatcher.IsSimilar(shorter, longer, minSimilarity)
                   || KeywordMatcher.Contains(longer, shorter, fuzzy: true);
        }
    }
}
