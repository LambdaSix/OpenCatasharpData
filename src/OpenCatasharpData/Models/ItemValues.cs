using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>A CDDA measurement or price: either a unit-bearing string or a JSON number.</summary>
[JsonConverter(typeof(CddaScalarConverter))]
public sealed record CddaScalar(string? Text = null, decimal? Number = null)
{
    public override string ToString() => Text ?? Number?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
}

public sealed class CddaScalarConverter : JsonConverter<CddaScalar>
{
    public override CddaScalar? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => new CddaScalar(Text: reader.GetString()),
            JsonTokenType.Number => new CddaScalar(Number: reader.GetDecimal()),
            JsonTokenType.Null => null,
            _ => throw new JsonException("Expected a CDDA unit string or number.")
        };

    public override void Write(Utf8JsonWriter writer, CddaScalar value, JsonSerializerOptions options)
    {
        if (value.Text is { } text) writer.WriteStringValue(text);
        else if (value.Number is { } number) writer.WriteNumberValue(number);
        else writer.WriteNullValue();
    }
}

/// <summary>One material in an item's composition, including optional portion and thickness.</summary>
public sealed class CddaMaterialPart
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("portion")]
    public decimal? Portion { get; set; }

    [JsonPropertyName("covered_by_mat")]
    public decimal? CoveredByMaterial { get; set; }

    [JsonPropertyName("thickness")]
    public decimal? Thickness { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Reads CDDA's mixed ["steel", {"type":"wood","portion":2}] material arrays.</summary>
public sealed class CddaMaterialListConverter : JsonConverter<List<CddaMaterialPart>>
{
    public override List<CddaMaterialPart>? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Null) return null;
        var materials = new List<CddaMaterialPart>();

        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in root.EnumerateArray()) materials.Add(ParseMaterial(entry, options));
        }
        else
        {
            materials.Add(ParseMaterial(root, options));
        }

        return materials;
    }

    private static CddaMaterialPart ParseMaterial(JsonElement element, JsonSerializerOptions options) =>
        element.ValueKind switch
        {
            JsonValueKind.String => new CddaMaterialPart { Type = element.GetString()! },
            JsonValueKind.Object => element.Deserialize<CddaMaterialPart>(options)
                ?? throw new JsonException("Invalid material object."),
            _ => throw new JsonException("Expected a material name or object.")
        };

    public override void Write(
        Utf8JsonWriter writer, List<CddaMaterialPart> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var part in value)
        {
            if (part.Portion is null && part.CoveredByMaterial is null
                && part.Thickness is null && (part.AdditionalProperties?.Count ?? 0) == 0)
                writer.WriteStringValue(part.Type);
            else JsonSerializer.Serialize(writer, part, options);
        }
        writer.WriteEndArray();
    }
}

/// <summary>Reads a string or a JSON array of strings into an eager list.</summary>
public sealed class CddaStringListConverter : JsonConverter<List<string>>
{
    public override List<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        return document.RootElement.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => [document.RootElement.GetString()!],
            JsonValueKind.Array => document.RootElement.EnumerateArray()
                .Select(value => value.ValueKind == JsonValueKind.String
                    ? value.GetString()!
                    : throw new JsonException("Expected a string list."))
                .ToList(),
            _ => throw new JsonException("Expected a string or string array.")
        };
    }

    public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, options);
}

/// <summary>A two-element CDDA tuple such as ["vitC", 167] or ["CUT", 2].</summary>
public sealed record CddaNamedAmount(string Name, decimal Amount);

public sealed class CddaNamedAmountsConverter : JsonConverter<List<CddaNamedAmount>>
{
    public override List<CddaNamedAmount>? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        if (document.RootElement.ValueKind == JsonValueKind.Null) return null;
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new JsonException("Expected an array of [name, amount] pairs.");

        var result = new List<CddaNamedAmount>();
        foreach (var tuple in document.RootElement.EnumerateArray())
        {
            if (tuple.ValueKind != JsonValueKind.Array || tuple.GetArrayLength() < 2)
                throw new JsonException("Expected [name, amount].");
            var name = tuple[0].GetString() ?? throw new JsonException("Name is required.");
            var amount = tuple[1].GetDecimal();
            result.Add(new CddaNamedAmount(name, amount));
        }
        return result;
    }

    public override void Write(Utf8JsonWriter writer, List<CddaNamedAmount> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var entry in value)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(entry.Name);
            writer.WriteNumberValue(entry.Amount);
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }
}

/// <summary>A scalar or a two-value range, e.g. armor encumbrance 10 or [5, 10].</summary>
[JsonConverter(typeof(CddaScalarRangeConverter))]
public sealed record CddaScalarRange(decimal? Minimum = null, decimal? Maximum = null, string? Text = null);

public sealed class CddaScalarRangeConverter : JsonConverter<CddaScalarRange>
{
    public override CddaScalarRange? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        return root.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.Number => new CddaScalarRange(root.GetDecimal(), root.GetDecimal()),
            JsonValueKind.String => new CddaScalarRange(Text: root.GetString()),
            JsonValueKind.Array when root.GetArrayLength() == 2 =>
                new CddaScalarRange(root[0].GetDecimal(), root[1].GetDecimal()),
            _ => throw new JsonException("Expected a scalar or two-number range.")
        };
    }

    public override void Write(Utf8JsonWriter writer, CddaScalarRange value, JsonSerializerOptions options)
    {
        if (value.Text is not null) writer.WriteStringValue(value.Text);
        else if (value.Minimum is { } min && value.Maximum is { } max && min == max)
            writer.WriteNumberValue(min);
        else if (value.Minimum is { } lower && value.Maximum is { } upper)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(lower);
            writer.WriteNumberValue(upper);
            writer.WriteEndArray();
        }
        else writer.WriteNullValue();
    }
}

public sealed class CddaArmorSection
{
    [JsonPropertyName("covers")]
    [JsonConverter(typeof(CddaStringListConverter))]
    public List<string>? Covers { get; set; }

    [JsonPropertyName("specifically_covers")]
    [JsonConverter(typeof(CddaStringListConverter))]
    public List<string>? SpecificallyCovers { get; set; }

    [JsonPropertyName("material")]
    [JsonConverter(typeof(CddaMaterialListConverter))]
    public List<CddaMaterialPart>? Material { get; set; }

    [JsonPropertyName("coverage")]
    public decimal? Coverage { get; set; }

    [JsonPropertyName("encumbrance")]
    public CddaScalarRange? Encumbrance { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

public sealed class CddaPocket
{
    [JsonPropertyName("pocket_type")]
    public string? PocketType { get; set; }

    [JsonPropertyName("max_contains_volume")]
    public CddaScalar? MaxContainsVolume { get; set; }

    [JsonPropertyName("max_contains_weight")]
    public CddaScalar? MaxContainsWeight { get; set; }

    [JsonPropertyName("ammo_restriction")]
    public Dictionary<string, int>? AmmoRestriction { get; set; }

    [JsonPropertyName("rigid")]
    public bool? Rigid { get; set; }

    [JsonPropertyName("watertight")]
    public bool? Watertight { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
