using System.Text;

namespace MapOptionGuard;

/// <summary>
/// OCR 줄들을 옵션 단위로 묶는다. 옵션 하나가 여러 줄로 줄바꿈되므로,
/// 글머리표(◆ 등)로 시작하거나 앞 줄이 문장 끝(…다, %, 중첩 등)으로 끝났을 때만 새 옵션으로 본다.
/// 앞 줄이 문장 끝처럼 보여도 조사로 시작하는 줄은 이어지는 줄로 본다.
/// </summary>
internal static class OptionLineParser
{
    private static readonly char[] BulletChars = ['◆', '◇', '♦', '•', '·', '●', '■', '▪', '*'];
    private static readonly string[] OptionEndings = ["다.", "다", "%", "중첩", "지속"];

    // "…경우 100%" 다음 줄 "의 확률로 …"처럼 조사 한 낱말로 시작하는 줄은 옵션 첫 줄일 수 없다
    private static readonly string[] ContinuationParticles = ["의", "을", "를", "에", "에게", "로", "으로", "만큼", "와", "과"];

    public static List<string> SplitOptions(IEnumerable<string> lines, string panelAnchor, bool fuzzy)
    {
        var options = new List<string>();
        var current = new StringBuilder();
        bool previousEnded = true;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || IsAnchorLine(line, panelAnchor, fuzzy))
            {
                continue;
            }

            bool startsNew = previousEnded && !IsContinuationLine(line);
            if (BulletChars.Contains(line[0]))
            {
                startsNew = true;
                line = line.TrimStart(BulletChars).TrimStart();
            }

            if (startsNew && current.Length > 0)
            {
                options.Add(current.ToString());
                current.Clear();
            }

            if (line.Length == 0)
            {
                // 글머리표만 따로 읽힌 줄이면 다음 줄부터 새 옵션이다
                previousEnded = true;
                continue;
            }

            if (current.Length > 0)
            {
                current.Append(' ');
            }

            current.Append(line);
            previousEnded = OptionEndings.Any(ending => line.EndsWith(ending, StringComparison.Ordinal));
        }

        if (current.Length > 0)
        {
            options.Add(current.ToString());
        }

        return options;
    }

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
