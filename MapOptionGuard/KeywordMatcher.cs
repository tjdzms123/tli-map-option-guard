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
    public static int AllowedMismatch(int length, bool fuzzy) =>
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

    /// <summary>띄어쓰기를 뺀 두 글자가 같은 길이이고 다른 글자 수가 허용치 이하인지.</summary>
    public static bool AreNearlyEqual(string compactA, string compactB, int allowedMismatch) =>
        compactA.Length == compactB.Length && CountMismatch(compactA, 0, compactB, allowedMismatch) <= allowedMismatch;

    public static List<string> FindBlocked(string compactText, IEnumerable<string> keywords, bool fuzzy) =>
        keywords.Where(keyword => Contains(compactText, keyword, fuzzy)).ToList();

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
