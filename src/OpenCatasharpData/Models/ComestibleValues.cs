using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>One addiction, represented in CDDA as a name or a richer object.</summary>
[JsonConverter(typeof(CddaAddictionJsonConverter))]
public sealed class CddaAddiction
{
    public string? Name { get; set; }
    public int? Potential { get; set; }
    public bool WasString { get; set; }
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

public sealed class CddaAddictionJsonConverter : JsonConverter<CddaAddiction>
{
    public override CddaAddiction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Null) return null;
        if (root.ValueKind == JsonValueKind.String)
            return new CddaAddiction { Name = root.GetString(), WasString = true };
        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException("Expected an addiction name or object.");

        var addiction = new CddaAddiction();
        foreach (var prop in root.EnumerateObject())
        {
            if (prop.NameEquals("addiction")) addiction.Name = prop.Value.GetString();
            else if (prop.NameEquals("potential")) addiction.Potential = prop.Value.GetInt32();
            else (addiction.AdditionalProperties ??= new())[prop.Name] = prop.Value.Clone();
        }
        return addiction;
    }

    public override void Write(Utf8JsonWriter writer, CddaAddiction value, JsonSerializerOptions options)
    {
        if (value.WasString)
        {
            writer.WriteStringValue(value.Name);
            return;
        }
        writer.WriteStartObject();
        if (value.Name is not null) writer.WriteString("addiction", value.Name);
        if (value.Potential is { } p) writer.WriteNumber("potential", p);
        if (value.AdditionalProperties is not null)
            foreach (var prop in value.AdditionalProperties)
            {
                writer.WritePropertyName(prop.Key);
                prop.Value.WriteTo(writer);
            }
        writer.WriteEndObject();
    }
}

/// <summary>
/// An effect-on-condition reference or inline definition.
/// Only known metadata is exposed as typed properties; arbitrary inline EOC
/// data remains preserved by extension data for later dedicated EOC models.
/// </summary>
[JsonConverter(typeof(CddaEffectReferenceJsonConverter))]
public sealed class CddaEffectReference
{
    public string? Id { get; set; }
    public bool WasString { get; set; }
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

public sealed class CddaEffectReferenceJsonConverter : JsonConverter<CddaEffectReference>
{
    public override CddaEffectReference? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Null) return null;
        if (root.ValueKind == JsonValueKind.String)
            return new CddaEffectReference { Id = root.GetString(), WasString = true };
        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException("Expected an effect reference name or inline object.");

        var result = new CddaEffectReference();
        foreach (var prop in root.EnumerateObject())
        {
            if (prop.NameEquals("id") && prop.Value.ValueKind == JsonValueKind.String)
                result.Id = prop.Value.GetString();
            else (result.AdditionalProperties ??= new())[prop.Name] = prop.Value.Clone();
        }
        return result;
    }

    public override void Write(Utf8JsonWriter writer, CddaEffectReference value, JsonSerializerOptions options)
    {
        if (value.WasString) { writer.WriteStringValue(value.Id); return; }
        writer.WriteStartObject();
        if (value.Id is not null) writer.WriteString("id", value.Id);
        if (value.AdditionalProperties is not null)
            foreach (var prop in value.AdditionalProperties)
            {
                writer.WritePropertyName(prop.Key);
                prop.Value.WriteTo(writer);
            }
        writer.WriteEndObject();
    }
}

/// <summary>Read a singular value or array of typed values uniformly.</summary>
public sealed class CddaOneOrManyJsonConverter<TValue> : JsonConverter<List<TValue>>
{
    public override List<TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Null) return null;
        if (root.ValueKind == JsonValueKind.Array)
            return root.EnumerateArray().Select(element =>
                element.Deserialize<TValue>(options)
                ?? throw new JsonException("Invalid list element.")).ToList();
        return [root.Deserialize<TValue>(options) ??
            throw new JsonException("Invalid singular list element.")];
    }

    public override void Write(Utf8JsonWriter writer, List<TValue> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, options);
}

public sealed class CddaContamination
{
    [JsonPropertyName("disease")]
    public string? Disease { get; set; }

    [JsonPropertyName("probability")]
    public decimal? Probability { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

public sealed class CddaRotSpawn
{
    [JsonPropertyName("group")]
    public string? Group { get; set; }

    [JsonPropertyName("monster")]
    public string? Monster { get; set; }

    [JsonPropertyName("chance")]
    public decimal? Chance { get; set; }

    [JsonPropertyName("amount")]
    public CddaScalarRange? Amount { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
