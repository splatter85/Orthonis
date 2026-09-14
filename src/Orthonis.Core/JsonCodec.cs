using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orthonis.Core;

public static class JsonCodec
{
    public const int MaxCaseBytes = 2 * 1024 * 1024;
    public const int MaxPlanBytes = 64 * 1024;
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        RespectNullableAnnotations = true,
        MaxDepth = 16,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
    };

    public static byte[] Encode<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, Options);
    public static JsonElement Element<T>(T value) => JsonSerializer.SerializeToElement(value, Options);
    public static string Hash(CaseSnapshot value) => Convert.ToHexString(SHA256.HashData(Encode(value))).ToLowerInvariant();

    public static T Decode<T>(byte[] bytes, int maxBytes = MaxCaseBytes)
    {
        Contract.Require(bytes.Length is > 0 && bytes.Length <= maxBytes, "JSON size limit exceeded or empty input.");
        try
        {
            using var doc = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 16 });
            Inspect(doc.RootElement);
            return JsonSerializer.Deserialize<T>(bytes, Options) ?? throw new RefusalException("Null JSON record.");
        }
        catch (JsonException) { throw new RefusalException("Malformed, missing, duplicate, or unsupported JSON fields."); }
        catch (NotSupportedException) { throw new RefusalException("Unsupported JSON representation."); }
    }

    private static void Inspect(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                Contract.Require(names.Add(property.Name), "Duplicate JSON property.");
                Inspect(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (var item in element.EnumerateArray()) Inspect(item);
        else if (element.ValueKind == JsonValueKind.Number)
            Contract.Require(element.TryGetDouble(out var n) && double.IsFinite(n), "Non-finite or excessive number.");
    }
}
