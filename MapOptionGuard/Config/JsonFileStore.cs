using System.Text.Encodings.Web;
using System.Text.Json;

namespace MapOptionGuard;

/// <summary>
/// 설정 파일과 옵션 수집 파일이 같은 JSON 형식(한글 그대로, 들여쓰기)을 쓰도록 모은 읽기/쓰기 도우미.
/// </summary>
internal static class JsonFileStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        // 한글을 \uXXXX 로 바꾸지 않고 그대로 저장해 직접 열어 보기 쉽게 한다
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>형식이 잘못되면 JsonException을 던진다.</summary>
    public static T? Load<T>(string path) => JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options);

    /// <summary>임시 파일에 먼저 쓰고 바꿔치기해서, 저장 도중 꺼져도 기존 파일이 깨지지 않게 한다.</summary>
    public static void Save<T>(string path, T value)
    {
        var tempPath = path + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(value, Options));
        File.Move(tempPath, path, overwrite: true);
    }
}
