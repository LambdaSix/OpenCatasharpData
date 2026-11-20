using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>
/// CDDA translatable text: either a JSON string or an object containing
/// str, str_sp, str_pl and/or ctxt. Raw JSON is retained for round-tripping.
/// </summary>
[JsonConverter(typeof(CddaTextJsonConverter))]
public sealed class CddaText
{
    internal CddaText(JsonElement raw) => Raw = raw.Clone();

    public JsonElement Raw { get; }

    public string? Singular =>
        Raw.ValueKind == JsonValueKind.String
            ? Raw.GetString()
            : GetString("str") ?? GetString("str_sp");

    public string? Plural => GetString("str_pl") ?? GetString("str_sp");

    public string? Context => GetString("ctxt");

    private string? GetString(string key) =>
        Raw.ValueKind == JsonValueKind.Object
        && Raw.TryGetProperty(key, out var value)
        && value.ValueKind == JsonValueKind.String
            ? value.GetString()
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
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        using var document = JsonDocument.ParseValue(ref reader);
        var raw = document.RootElement;
        if (raw.ValueKind is not (JsonValueKind.String or JsonValueKind.Object))
        {
            throw new JsonException("CDDA text must be a string or translation object.");
        }

        return new CddaText(raw);
    }

    public override void Write(
        Utf8JsonWriter writer,
        CddaText value,
        JsonSerializerOptions options) => value.Raw.WriteTo(writer);
}
