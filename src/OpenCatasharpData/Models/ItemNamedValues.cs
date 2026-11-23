using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>
/// A tool quality is either a compact [id, level] tuple or a richer object
/// such as {"id":"TEMPER","level":2,"speed":1.1}.
/// </summary>
public sealed class CddaQuality
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public decimal? Level { get; set; }

    [JsonPropertyName("speed")]
    public decimal? Speed { get; set; }

    [JsonIgnore]
    public bool WasTuple { get; internal set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

public sealed class CddaQualitiesJsonConverter : JsonConverter<List<CddaQuality>>
{
    public override List<CddaQuality>? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Null) return null;
        if (root.ValueKind != JsonValueKind.Array)
            throw new JsonException("Qualities must be an array.");

        var result = new List<CddaQuality>();
        foreach (var entry in root.EnumerateArray())
        {
            if (entry.ValueKind == JsonValueKind.Object)
                result.Add(entry.Deserialize<CddaQuality>(options)
                           ?? throw new JsonException("Invalid quality object."));
            else if (entry.ValueKind == JsonValueKind.Array && entry.GetArrayLength() == 2)
                result.Add(new CddaQuality
                {
                    Id = entry[0].GetString() ?? throw new JsonException("Quality ID is required."),
                    Level = entry[1].GetDecimal(),
                    WasTuple = true
                });
            else throw new JsonException("Quality must be [id, level] or an object.");
        }
        return result;
    }

    public override void Write(
        Utf8JsonWriter writer, List<CddaQuality> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var entry in value)
        {
            if (entry.WasTuple)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(entry.Id);
                writer.WriteNumberValue(entry.Level ?? 0);
                writer.WriteEndArray();
            }
            else JsonSerializer.Serialize(writer, entry, options);
        }
        writer.WriteEndArray();
    }
}

/// <summary>Vitamins have numeric or unit-bearing quantities: ["vitC","83700 μg"].</summary>
public sealed record CddaVitamin(string Name, CddaScalar Value);

public sealed class CddaVitaminsJsonConverter : JsonConverter<List<CddaVitamin>>
{
    public override List<CddaVitamin>? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Null) return null;
        if (root.ValueKind != JsonValueKind.Array)
            throw new JsonException("Vitamins must be an array.");

        var result = new List<CddaVitamin>();
        foreach (var tuple in root.EnumerateArray())
        {
            if (tuple.ValueKind != JsonValueKind.Array || tuple.GetArrayLength() != 2)
                throw new JsonException("Vitamin must be [id, amount].");
            var name = tuple[0].GetString() ?? throw new JsonException("Vitamin ID is required.");
            var value = tuple[1].Deserialize<CddaScalar>(options)
                ?? throw new JsonException("Vitamin amount is required.");
            result.Add(new CddaVitamin(name, value));
        }
        return result;
    }

    public override void Write(
        Utf8JsonWriter writer, List<CddaVitamin> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(item.Name);
            JsonSerializer.Serialize(writer, item.Value, options);
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }
}
