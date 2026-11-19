namespace MapOptionGuard;

/// <summary>
/// OCR 결과에서 키워드를 찾는다. 줄바꿈·띄어쓰기는 무시하고, 퍼지 모드에서는
/// 4글자 이상 키워드에 한해 OCR 오인식을 허용한다(15글자마다 한 글자씩 더).
/// </summary>
internal static class KeywordMatcher
{
    private const int FuzzyMinLength = 4;
    private const int CharsPerExtraMismatch = 15;

    public static string RemoveWhitespace(string text) =>
        new(text.Where(c => !char.IsWhiteSpace(c)).ToArray());

    /// <summary>길이가 length인 글자를 비교할 때 다르게 읽혀도 같다고 볼 글자 수.</summary>
    private static int AllowedMismatch(int length, bool fuzzy) =>
        fuzzy && length >= FuzzyMinLength ? Math.Max(1, length / CharsPerExtraMismatch) : 0;

    /// <param name="compactText">RemoveWhitespace를 거친 OCR 결과</param>
    public static bool Contains(string compactText, string keyword, bool fuzzy)
    {
        var key = RemoveWhitespace(keyword);
        if (key.Length == 0)
        {
            return false;
        }

        int allowedMismatch = AllowedMismatch(key.Length, fuzzy);
        if (allowedMismatch == 0)
        {
            return compactText.Contains(key, StringComparison.Ordinal);
        }

        // 키워드 길이만큼 밀어가며, 다른 글자 수가 허용치 이하인 구간이 있는지 본다
        for (int start = 0; start + key.Length <= compactText.Length; start++)
        {
            if (CountMismatch(compactText, start, key, allowedMismatch) <= allowedMismatch)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 띄어쓰기를 뺀 두 글자가 같은 옵션으로 볼 만큼 비슷한지. 비슷한 정도는 1 - 편집 거리 / 긴 쪽 길이(0~1)이다.
    /// OCR이 글자를 바꿔 읽거나 빼먹고 더 읽은 경우를 모두 같은 옵션으로 본다.
    /// </summary>
    public static bool IsSimilar(string compactA, string compactB, double minSimilarity)
    {
        int longer = Math.Max(compactA.Length, compactB.Length);
        if (longer == 0)
        {
            return true;
        }

        // 길이 차이만으로도 기준에 못 미치면 편집 거리를 계산하지 않는다
        // 1 - 0.8이 0.19999…로 계산되어 한 글자 덜 허용되지 않도록 아주 작은 값을 더한다
        int maxDistance = (int)Math.Floor(longer * (1 - minSimilarity) + 1e-9);
        return Math.Abs(compactA.Length - compactB.Length) <= maxDistance
               && EditDistance(compactA, compactB) <= maxDistance;
    }

    public static List<string> FindBlocked(string compactText, IEnumerable<string> keywords, bool fuzzy) =>
        keywords.Where(keyword => Contains(compactText, keyword, fuzzy)).ToList();

    // 한 글자 바꾸기·넣기·빼기를 한 번으로 셀 때, a를 b로 바꾸는 데 드는 최소 횟수(레벤슈타인 거리)
    private static int EditDistance(string a, string b)
    {
        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];
        for (int j = 0; j <= b.Length; j++)
        {
            previous[j] = j;
        }

        for (int i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            for (int j = 1; j <= b.Length; j++)
            {
                int replace = previous[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1);
                current[j] = Math.Min(replace, Math.Min(previous[j], current[j - 1]) + 1);
            }

            (previous, current) = (current, previous);
        }

        return previous[b.Length];
    }

    // text[start..]와 key를 앞에서부터 비교해 다른 글자 수를 센다. limit를 넘으면 더 세지 않는다.
    private static int CountMismatch(string text, int start, string key, int limit)
    {
        int mismatch = 0;
        for (int i = 0; i < key.Length && mismatch <= limit; i++)
        {
            if (text[start + i] != key[i])
            {
                mismatch++;
            }
        }

        return mismatch;
    }
}
