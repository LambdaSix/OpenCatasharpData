using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>An item proficiency with optional learning time/failure modifiers.</summary>
public sealed class CddaItemProficiency
{
    [JsonPropertyName("proficiency")]
    public string? Proficiency { get; set; }

    [JsonPropertyName("time_factor")]
    public decimal? TimeFactor { get; set; }

    [JsonPropertyName("fail_factor")]
    public decimal? FailFactor { get; set; }

    [JsonPropertyName("required")]
    public bool? Required { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Inline item description snippet. A standalone string is preserved as a string.</summary>
public sealed class CddaSnippet
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("text")]
    public CddaText? Text { get; set; }

    [JsonIgnore]
    public bool WasString { get; internal set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>
/// An item snippet_category is either a category ID or a list of inline snippets.
/// The converter preserves the source's string or array shape.
/// </summary>
[JsonConverter(typeof(CddaSnippetCategoryJsonConverter))]
public sealed class CddaSnippetCategory
{
    public string? CategoryId { get; set; }
    public List<CddaSnippet>? Snippets { get; set; }
    public bool WasString { get; internal set; }
}

public sealed class CddaSnippetCategoryJsonConverter : JsonConverter<CddaSnippetCategory>
{
    public override CddaSnippetCategory? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Null) return null;
        if (root.ValueKind == JsonValueKind.String)
            return new CddaSnippetCategory { CategoryId = root.GetString(), WasString = true };
        if (root.ValueKind != JsonValueKind.Array)
            throw new JsonException("Expected a snippet category ID or snippet array.");

        var result = new CddaSnippetCategory { Snippets = [] };
        foreach (var entry in root.EnumerateArray())
        {
            if (entry.ValueKind == JsonValueKind.String)
                result.Snippets.Add(new CddaSnippet { Id = entry.GetString(), WasString = true });
            else if (entry.ValueKind == JsonValueKind.Object)
                result.Snippets.Add(entry.Deserialize<CddaSnippet>(options)
                    ?? throw new JsonException("Invalid item snippet."));
            else
                throw new JsonException("Expected inline item snippet objects or strings.");
        }
        return result;
    }

    public override void Write(
        Utf8JsonWriter writer, CddaSnippetCategory value, JsonSerializerOptions options)
    {
        if (value.WasString)
        {
            writer.WriteStringValue(value.CategoryId);
            return;
        }
        writer.WriteStartArray();
        foreach (var entry in value.Snippets ?? [])
        {
            if (entry.WasString)
                writer.WriteStringValue(entry.Id);
            else
                JsonSerializer.Serialize(writer, entry, options);
        }
        writer.WriteEndArray();
    }
}

/// <summary>
/// One countdown-triggered item action, either string or object.
/// Unlike a use_action list, this retains a singular JSON field shape.
/// </summary>
[JsonConverter(typeof(CddaItemActionJsonConverter))]
public sealed class CddaItemAction
{
    public CddaUseAction? Action { get; set; }
    public bool WasString { get; internal set; }
}

public sealed class CddaItemActionJsonConverter : JsonConverter<CddaItemAction>
{
    public override CddaItemAction? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Null) return null;
        if (root.ValueKind == JsonValueKind.String)
            return new CddaItemAction
            {
                WasString = true,
                Action = new CddaUseAction { Type = root.GetString(), WasString = true }
            };
        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException("Expected countdown action as a string or object.");
        return new CddaItemAction
        {
            Action = root.Deserialize<CddaUseAction>(options)
                ?? throw new JsonException("Invalid countdown action.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer, CddaItemAction value, JsonSerializerOptions options)
    {
        if (value.WasString)
            writer.WriteStringValue(value.Action?.Type);
        else if (value.Action is null)
            writer.WriteNullValue();
        else
            JsonSerializer.Serialize(writer, value.Action, options);
    }
}

/// <summary>Item action that changes into another item and optional container.</summary>
public sealed class CddaItemTransformation
{
    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("container")]
    public string? Container { get; set; }

    [JsonPropertyName("target_charges")]
    public CddaScalar? TargetCharges { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Optional memory-card content and recipe discovery metadata.</summary>
public sealed class CddaMemoryCard
{
    [JsonPropertyName("data_chance")]
    public decimal? DataChance { get; set; }

    [JsonPropertyName("recipes_chance")]
    public decimal? RecipesChance { get; set; }

    [JsonPropertyName("recipes_amount")]
    public int? RecipesAmount { get; set; }

    [JsonPropertyName("recipes_categories")]
    public List<string>? RecipeCategories { get; set; }

    [JsonPropertyName("recipes_level_max")]
    public int? MaxRecipeLevel { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
