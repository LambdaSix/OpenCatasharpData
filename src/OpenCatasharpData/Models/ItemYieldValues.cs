using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>
/// One output item from brewing/composting. Quantity can be numeric or textual,
/// and variant is populated only for the object form.
/// </summary>
public sealed class CddaItemYield
{
    [JsonPropertyName("item")]
    public string Item { get; set; } = string.Empty;

    [JsonPropertyName("variant")]
    public string? Variant { get; set; }

    [JsonPropertyName("count")]
    public CddaScalar? Count { get; set; }

    [JsonIgnore]
    public bool WasString { get; internal set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>
/// Preserves CDDA's alternative result formats:
/// ["product", {"item":"yeast","count":3}] or {"product":2,"yeast":3}.
/// All results are eager typed and retain input collection shape.
/// </summary>
[JsonConverter(typeof(CddaItemYieldSetJsonConverter))]
public sealed class CddaItemYieldSet
{
    public List<CddaItemYield> Results { get; set; } = [];

    public bool WasMap { get; internal set; }
}

public sealed class CddaItemYieldSetJsonConverter : JsonConverter<CddaItemYieldSet>
{
    public override CddaItemYieldSet? Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var element = doc.RootElement;
        if (element.ValueKind == JsonValueKind.Null) return null;
        var result = new CddaItemYieldSet();

        if (element.ValueKind == JsonValueKind.Object)
        {
            result.WasMap = true;
            foreach (var pair in element.EnumerateObject())
                result.Results.Add(new CddaItemYield
                {
                    Item = pair.Name,
                    Count = pair.Value.Deserialize<CddaScalar>(options)
                });
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in element.EnumerateArray())
            {
                if (entry.ValueKind == JsonValueKind.String)
                    result.Results.Add(new CddaItemYield
                    {
                        Item = entry.GetString()!,
                        WasString = true
                    });
                else if (entry.ValueKind == JsonValueKind.Object)
                    result.Results.Add(entry.Deserialize<CddaItemYield>(options)
                        ?? throw new JsonException("Invalid result item."));
                else
                    throw new JsonException("Item result must be a string or object.");
            }
        }
        else throw new JsonException("Item results must be a map or an array.");

        return result;
    }

    public override void Write(Utf8JsonWriter writer,
        CddaItemYieldSet value, JsonSerializerOptions options)
    {
        if (value.WasMap)
        {
            writer.WriteStartObject();
            foreach (var output in value.Results)
            {
                writer.WritePropertyName(output.Item);
                JsonSerializer.Serialize(writer, output.Count, options);
            }
            writer.WriteEndObject();
        }
        else
        {
            writer.WriteStartArray();
            foreach (var output in value.Results)
            {
                if (output.WasString) writer.WriteStringValue(output.Item);
                else JsonSerializer.Serialize(writer, output, options);
            }
            writer.WriteEndArray();
        }
    }
}
