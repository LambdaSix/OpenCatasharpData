using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>
/// Raw CDDA definition. The JSON ID can be a string or a more complex shape,
/// depending on the type, so the original value is preserved.
/// </summary>
public abstract class CddaDefinition
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public JsonElement? Id { get; set; }

    [JsonIgnore]
    public string? IdString =>
        Id is { ValueKind: JsonValueKind.String } id ? id.GetString() : null;

    [JsonPropertyName("abstract")]
    public string? Abstract { get; set; }

    [JsonPropertyName("copy-from")]
    public string? CopyFrom { get; set; }

    [JsonPropertyName("extend")]
    public JsonElement? Extend { get; set; }

    [JsonPropertyName("delete")]
    public JsonElement? Delete { get; set; }

    [JsonPropertyName("relative")]
    public JsonElement? Relative { get; set; }

    [JsonPropertyName("proportional")]
    public JsonElement? Proportional { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A definition with a discriminator not yet mapped to a typed DTO.</summary>
public sealed class UnknownDefinition : CddaDefinition
{
}
