using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

public sealed class SkillDefinition : CddaDefinition
{
    [JsonPropertyName("name")]
    public CddaText? Name { get; set; }

    [JsonPropertyName("description")]
    public CddaText? Description { get; set; }

    [JsonPropertyName("display_category")]
    public string? DisplayCategory { get; set; }

    [JsonPropertyName("sort_rank")]
    public int? SortRank { get; set; }

    [JsonPropertyName("level_descriptions_theory")]
    public List<SkillLevelDescription>? LevelDescriptionsTheory { get; set; }

    [JsonPropertyName("level_descriptions_practice")]
    public List<SkillLevelDescription>? LevelDescriptionsPractice { get; set; }
}

public sealed class SkillLevelDescription
{
    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("description")]
    public CddaText? Description { get; set; }
}

public sealed class MaterialDefinition : CddaDefinition
{
    [JsonPropertyName("name")]
    public CddaText? Name { get; set; }

    [JsonPropertyName("density")]
    public double? Density { get; set; }

    [JsonPropertyName("chip_resist")]
    public int? ChipResist { get; set; }

    [JsonPropertyName("conductive")]
    public bool? Conductive { get; set; }

    [JsonPropertyName("salvaged_into")]
    public string? SalvagedInto { get; set; }
}

public sealed class ItemCategoryDefinition : CddaDefinition
{
    [JsonPropertyName("name_header")]
    public CddaText? NameHeader { get; set; }

    [JsonPropertyName("name_noun")]
    public CddaText? NameNoun { get; set; }

    [JsonPropertyName("zone")]
    public string? Zone { get; set; }

    [JsonPropertyName("sort_rank")]
    public int? SortRank { get; set; }
}

public sealed class AmmunitionTypeDefinition : CddaDefinition
{
    [JsonPropertyName("name")]
    public CddaText? Name { get; set; }

    [JsonPropertyName("default")]
    public string? DefaultItem { get; set; }
}

/// <summary>
/// Common item properties only. Subtype-specific shapes are kept in
/// AdditionalProperties until dedicated DTOs are introduced.
/// </summary>
public sealed class ItemDefinition : CddaDefinition
{
    [JsonPropertyName("name")]
    public CddaText? Name { get; set; }

    [JsonPropertyName("description")]
    public CddaText? Description { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("weight")]
    public JsonElement? Weight { get; set; }

    [JsonPropertyName("volume")]
    public JsonElement? Volume { get; set; }

    [JsonPropertyName("price")]
    public JsonElement? Price { get; set; }

    [JsonPropertyName("material")]
    public JsonElement? Material { get; set; }

    [JsonPropertyName("flags")]
    public JsonElement? Flags { get; set; }
}

public sealed class RecipeDefinition : CddaDefinition
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("subcategory")]
    public string? Subcategory { get; set; }

    [JsonPropertyName("difficulty")]
    public int? Difficulty { get; set; }

    [JsonPropertyName("time")]
    public JsonElement? Time { get; set; }
}

public sealed class MonsterDefinition : CddaDefinition
{
    [JsonPropertyName("name")]
    public CddaText? Name { get; set; }

    [JsonPropertyName("description")]
    public CddaText? Description { get; set; }

    [JsonPropertyName("symbol")]
    public JsonElement? Symbol { get; set; }

    [JsonPropertyName("hp")]
    public JsonElement? HitPoints { get; set; }

    [JsonPropertyName("speed")]
    public JsonElement? Speed { get; set; }
}
