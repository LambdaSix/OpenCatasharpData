using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>Common raw definition metadata, including source inheritance directives.</summary>
public abstract class CddaDefinition
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    // Some CDDA kinds use array/object IDs. Retain the original shape.
    [JsonPropertyName("id")]
    public JsonElement? Id { get; set; }

    /// <summary>String ID captured eagerly by the record parser.</summary>
    [JsonIgnore]
    public string? IdString { get; internal set; }

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

/// <summary>Source definition not matched by a registered handler.</summary>
public sealed class UnknownDefinition : CddaDefinition
{
}
