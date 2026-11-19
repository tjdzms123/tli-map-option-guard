namespace MapOptionGuard;

/// <summary>옵션 하나. 창 위·아래에서 잘렸을 수 있거나 OCR이 깨지게 읽은 옵션은 모으지 않는다.</summary>
/// <param name="IsComplete">처음부터 끝까지 다 보였는지. 창 위·아래에서 잘렸을 수 있으면 false.</param>
internal sealed record ParsedOption(string Text, bool IsComplete)
{
    public bool IsClean => OptionLineParser.IsCleanText(Text);

    /// <summary>온전히 보이고 제대로 읽혀서 옵션 목록에 모아도 되는지</summary>
    public bool CanCollect => IsComplete && IsClean;
}

/// <summary>
/// OCR 줄들을 옵션 단위로 묶는다. 게임은 옵션마다 첫 줄 앞에 빨간 ◆를 붙이고, 길면 다음 줄로 줄바꿈한다.
/// 그래서 ◆가 있는 줄부터 다음 ◆ 전까지를 옵션 하나로 본다.
/// ◆를 하나도 찾지 못한 화면에서만 문장 끝(…다, %, 지속 등)을 기준으로 나눈다.
/// </summary>
internal static class OptionLineParser
{
    // OCR이 글머리표를 글자로 읽은 경우
    private static readonly char[] BulletChars = ['◆', '◇', '♦', '•', '·', '●', '■', '▪', '*'];

    // ◆를 못 찾았을 때, 줄이 이 말로 끝나면 다음 줄부터 새 옵션으로 본다
    private static readonly string[] LineBreakEndings = ["다.", "다", "%", "중첩", "지속"];

    // 맨 아래 옵션이 이 말로 끝나면 창 아래에서 잘리지 않고 끝난 것으로 본다
    private static readonly string[] OptionEndings = ["다.", "다", "%", "중첩", "지속", "가능", "회복", "획득", "추가", ")", "회", "초"];

    // "…경우 100%" 다음 줄 "의 확률로 …"처럼 조사 한 낱말로 시작하는 줄은 옵션 첫 줄일 수 없다
    private static readonly string[] ContinuationParticles = ["의", "을", "를", "에", "에게", "로", "으로", "만큼", "와", "과"];

    // 옵션 글자에 쓰이는 문장부호. 이 밖의 기호(|, * 등)나 한자·자모가 섞이면 OCR이 깨지게 읽은 것으로 본다.
    private const string AllowedPunctuation = "+-%.,()[]:/~'\"·×!?";

    public static List<ParsedOption> SplitOptions(IReadOnlyList<OcrTextLine> lines, string panelAnchor, bool fuzzy)
    {
        var content = new List<OcrTextLine>();
        foreach (var line in lines)
        {
            var text = line.Text.Trim();
            bool startsWithBullet = line.StartsWithBullet;
            if (text.Length > 0 && BulletChars.Contains(text[0]))
            {
                startsWithBullet = true;
                text = text.TrimStart(BulletChars).TrimStart();
            }

            // 글머리표만 따로 읽힌 줄은 남겨서 다음 줄부터 새 옵션이 되게 한다
            if ((text.Length == 0 && !startsWithBullet) || IsAnchorLine(text, panelAnchor, fuzzy))
            {
                continue;
            }

            content.Add(line with { Text = text, StartsWithBullet = startsWithBullet });
        }

        bool hasBullets = content.Any(line => line.StartsWithBullet);
        var groups = hasBullets ? GroupByBullets(content) : GroupByEndings(content);
        return ToOptions(groups, requireBullet: hasBullets);
    }

    /// <summary>
    /// 한글·숫자·옵션에 쓰는 문장부호만 있는지. 한자·자모·이상한 기호나 한 글자짜리 영문(E| 등)이 섞이면
    /// OCR이 깨지게 읽은 것이다. 영문은 HP처럼 두 글자 이상 이어진 것만 옵션 글자로 본다.
    /// </summary>
    public static bool IsCleanText(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if ((c is >= '가' and <= '힣') || char.IsAsciiDigit(c) || char.IsWhiteSpace(c) || AllowedPunctuation.Contains(c))
            {
                continue;
            }

            if (!char.IsAsciiLetter(c))
            {
                return false;
            }

            int start = i;
            while (i + 1 < text.Length && char.IsAsciiLetter(text[i + 1]))
            {
                i++;
            }

            if (i == start)
            {
                return false;
            }
        }

        return true;
    }

    // ◆가 있는 줄부터 다음 ◆ 전까지가 옵션 하나다. 첫 ◆ 앞 줄들은 따로 한 묶음으로 둔다.
    private static List<List<OcrTextLine>> GroupByBullets(List<OcrTextLine> lines)
    {
        var groups = new List<List<OcrTextLine>>();
        foreach (var line in lines)
        {
            if (line.StartsWithBullet || groups.Count == 0)
            {
                groups.Add([]);
            }

            groups[^1].Add(line);
        }

        return groups;
    }

    // ◆를 못 찾았을 때: 앞 줄이 문장 끝으로 끝났고 이번 줄이 조사로 시작하지 않으면 새 옵션으로 본다
    private static List<List<OcrTextLine>> GroupByEndings(List<OcrTextLine> lines)
    {
        var groups = new List<List<OcrTextLine>>();
        bool previousEnded = true;
        foreach (var line in lines)
        {
            if (groups.Count == 0 || (previousEnded && !IsContinuationLine(line.Text)))
            {
                groups.Add([]);
            }

            groups[^1].Add(line);
            previousEnded = EndsWithAny(line.Text, LineBreakEndings);
        }

        return groups;
    }

    /// <summary>
    /// 줄 묶음을 옵션으로 만든다. ◆ 없이 시작한 묶음(창 위로 잘린 옵션의 뒷부분, 드롭 수량 같은 머리 줄)과
    /// 창 아래에서 잘렸을 수 있는 맨 아래 옵션은 온전하지 않은 것으로 표시한다.
    /// </summary>
    private static List<ParsedOption> ToOptions(List<List<OcrTextLine>> groups, bool requireBullet)
    {
        var options = new List<ParsedOption>();
        for (int i = 0; i < groups.Count; i++)
        {
            var group = groups[i];
            var text = string.Join(" ", group.Select(line => line.Text).Where(t => t.Length > 0));
            if (text.Length == 0)
            {
                continue;
            }

            bool startsWell = !requireBullet || group[0].StartsWithBullet;
            bool endsWell = i < groups.Count - 1 || group[^1].HasSpaceBelow || EndsWithAny(text, OptionEndings);
            options.Add(new ParsedOption(text, startsWell && endsWell));
        }

        return options;
    }

    private static bool EndsWithAny(string text, string[] endings) =>
        endings.Any(ending => text.EndsWith(ending, StringComparison.Ordinal));

    private static bool IsContinuationLine(string line) =>
        ContinuationParticles.Any(particle => StartsWithWord(line, particle));

    // "의 확률로"는 맞고 "의식"처럼 낱말 일부인 경우는 아니다
    private static bool StartsWithWord(string line, string word) =>
        line.StartsWith(word, StringComparison.Ordinal)
        && (line.Length == word.Length || char.IsWhiteSpace(line[word.Length]));

    // '기본 옵션' 같은 창 제목 줄은 옵션이 아니다
    private static bool IsAnchorLine(string line, string panelAnchor, bool fuzzy) =>
        !string.IsNullOrWhiteSpace(panelAnchor)
        && KeywordMatcher.Contains(KeywordMatcher.RemoveWhitespace(line), panelAnchor, fuzzy);
}
