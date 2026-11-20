using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>
/// CDDA translatable text. Singular, Plural and Context are extracted eagerly
/// when JSON is read; the original value is also retained for lossless writing.
/// </summary>
[JsonConverter(typeof(CddaTextJsonConverter))]
public sealed class CddaText
{
    internal CddaText(JsonElement raw)
    {
        Raw = raw.Clone();

        if (raw.ValueKind == JsonValueKind.String)
        {
            Singular = raw.GetString();
        }
        else
        {
            Singular = GetString(raw, "str") ?? GetString(raw, "str_sp");
            Plural = GetString(raw, "str_pl") ?? GetString(raw, "str_sp");
            Context = GetString(raw, "ctxt");
        }
    }

    public JsonElement Raw { get; }
    public string? Singular { get; }
    public string? Plural { get; }
    public string? Context { get; }

    private static string? GetString(JsonElement value, string key) =>
        value.ValueKind == JsonValueKind.Object
        && value.TryGetProperty(key, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    public override string ToString() => Singular ?? string.Empty;
}

public sealed class CddaTextJsonConverter : JsonConverter<CddaText>
{
    public override CddaText? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;

        using var document = JsonDocument.ParseValue(ref reader);
        var raw = document.RootElement;
        if (raw.ValueKind is not (JsonValueKind.String or JsonValueKind.Object))
            throw new JsonException("CDDA text must be a string or translation object.");

        return new CddaText(raw);
    }

    public override void Write(
        Utf8JsonWriter writer,
        CddaText value,
        JsonSerializerOptions options) => value.Raw.WriteTo(writer);
}
