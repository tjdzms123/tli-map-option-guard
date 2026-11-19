namespace MapOptionGuard;

/// <summary>
/// OCR 결과에서 키워드를 찾는다. 줄바꿈·띄어쓰기는 무시하고,
/// 퍼지 모드에서는 4글자 이상 키워드에 한해 한 글자 오인식을 허용한다.
/// </summary>
internal static class KeywordMatcher
{
    private const int FuzzyMinLength = 4;

    public static string RemoveWhitespace(string text) =>
        new(text.Where(c => !char.IsWhiteSpace(c)).ToArray());

    /// <param name="compactText">RemoveWhitespace를 거친 OCR 결과</param>
    public static bool Contains(string compactText, string keyword, bool fuzzy)
    {
        var key = RemoveWhitespace(keyword);
        if (key.Length == 0)
        {
            return false;
        }

        int allowedMismatch = fuzzy && key.Length >= FuzzyMinLength ? 1 : 0;
        return allowedMismatch == 0
            ? compactText.Contains(key, StringComparison.Ordinal)
            : ContainsWithMismatch(compactText, key, allowedMismatch);
    }

    public static List<string> FindBlocked(string compactText, IEnumerable<string> keywords, bool fuzzy) =>
        keywords.Where(keyword => Contains(compactText, keyword, fuzzy)).ToList();

    // 키워드 길이만큼 밀어가며, 다른 글자 수가 허용치 이하인 구간이 있는지 본다
    private static bool ContainsWithMismatch(string text, string key, int allowedMismatch)
    {
        for (int start = 0; start + key.Length <= text.Length; start++)
        {
            int mismatch = 0;
            for (int i = 0; i < key.Length && mismatch <= allowedMismatch; i++)
            {
                if (text[start + i] != key[i])
                {
                    mismatch++;
                }
            }

            if (mismatch <= allowedMismatch)
            {
                return true;
            }
        }

        return false;
    }
}
