using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>A CBM installation item. The corresponding bionic effect is identified by the ITEM ID.</summary>
public sealed class BionicItemData
{
    [JsonPropertyName("difficulty")]
    public int? Difficulty { get; set; }

    [JsonPropertyName("is_upgrade")]
    public bool? IsUpgrade { get; set; }
}

/// <summary>Seed growth stages, harvest outputs and soil/temperature constraints.</summary>
public sealed class SeedData
{
    [JsonPropertyName("plant_name")]
    public string? PlantName { get; set; }

    [JsonPropertyName("fruit")]
    public string? Fruit { get; set; }

    [JsonPropertyName("seeds")]
    public bool? ProducesSeeds { get; set; }

    [JsonPropertyName("byproducts")]
    public List<string>? Byproducts { get; set; }

    // Each stage is a one-key map, such as { "GROWTH_SEED": "5 days" }.
    // CddaDuration eagerly parses the value while retaining its source unit.
    [JsonPropertyName("growth_stages")]
    public List<Dictionary<string, CddaDuration>>? GrowthStages { get; set; }

    [JsonPropertyName("growth_temp")]
    public CddaTemperature? GrowthTemperature { get; set; }

    [JsonPropertyName("required_terrain_flag")]
    public string? RequiredTerrainFlag { get; set; }
}

/// <summary>Liquid or solid fermentation time and resultant item quantities.</summary>
public sealed class BrewableData
{
    [JsonPropertyName("brew_time")]
    public CddaDuration? BrewTime { get; set; }

    [JsonPropertyName("brew_results")]
    public CddaItemYieldSet? BrewResults { get; set; }
}

/// <summary>Composting time and resultant item quantities.</summary>
public sealed class CompostableData
{
    [JsonPropertyName("compost_time")]
    public CddaDuration? CompostTime { get; set; }

    [JsonPropertyName("compost_results")]
    public CddaItemYieldSet? CompostResults { get; set; }
}

/// <summary>Milling transformation and associated crafting recipe.</summary>
public sealed class MillingData
{
    [JsonPropertyName("into")]
    public string? ResultItem { get; set; }

    [JsonPropertyName("recipe")]
    public string? Recipe { get; set; }
}

/// <summary>Vehicle engine subtype (its shared item and fault fields remain on ItemDefinition).</summary>
public sealed class EngineData
{
}

/// <summary>Vehicle wheel footprint in CDDA's source-defined numeric units.</summary>
public sealed class WheelData
{
    [JsonPropertyName("diameter")]
    public decimal? Diameter { get; set; }

    [JsonPropertyName("width")]
    public decimal? Width { get; set; }
}

/// <summary>Pet armour fitting information. Volumes are strongly unit typed.</summary>
public sealed class PetArmorData
{
    [JsonPropertyName("min_pet_vol")]
    public CddaVolume? MinimumPetVolume { get; set; }

    [JsonPropertyName("max_pet_vol")]
    public CddaVolume? MaximumPetVolume { get; set; }

    [JsonPropertyName("pet_bodytype")]
    public string? PetBodyType { get; set; }

    [JsonPropertyName("material_thickness")]
    public decimal? MaterialThickness { get; set; }

    [JsonPropertyName("environmental_protection")]
    public decimal? EnvironmentalProtection { get; set; }
}

/// <summary>Tool modification compatibility and optional pocket migration.</summary>
public sealed class ToolModData
{
    [JsonPropertyName("acceptable_ammo")]
    [JsonConverter(typeof(CddaStringListConverter))]
    public List<string>? AcceptableAmmo { get; set; }

    [JsonPropertyName("ammo_modifier")]
    [JsonConverter(typeof(CddaStringListConverter))]
    public List<string>? AmmoModifier { get; set; }

    [JsonPropertyName("magazine_adaptor")]
    public List<CddaMagazineAdaptor>? MagazineAdaptors { get; set; }

    [JsonPropertyName("pocket_mods")]
    public List<CddaPocket>? PocketMods { get; set; }

    [JsonPropertyName("consumption_mods")]
    public List<CddaPocketModifier>? ConsumptionMods { get; set; }

    [JsonPropertyName("capacity_mods")]
    public List<CddaPocketModifier>? CapacityMods { get; set; }
}

/// <summary>One numeric adjustment granted by an artifact's passive effect.</summary>
public sealed class CddaArtifactAdjustment
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("add")]
    public decimal? Add { get; set; }

    [JsonPropertyName("multiply")]
    public decimal? Multiply { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Artifact passive condition and its list of source-defined adjustments.</summary>
public sealed class CddaArtifactPassiveEffect
{
    /// <summary>
    /// Existing named enchantment reference. Alternatively, inline
    /// properties below specify an effect directly.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonIgnore]
    public bool IsReference => Id is not null;

    [JsonPropertyName("has")]
    public string? Has { get; set; }

    [JsonPropertyName("condition")]
    public string? Condition { get; set; }

    [JsonPropertyName("values")]
    public List<CddaArtifactAdjustment>? Values { get; set; }

    [JsonPropertyName("mutations")]
    public List<string>? Mutations { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Passive and active enchantments of an ARTIFACT item.</summary>
public sealed class ArtifactData
{
    [JsonPropertyName("passive_effects")]
    public List<CddaArtifactPassiveEffect>? PassiveEffects { get; set; }

    [JsonPropertyName("effects_carried")]
    public List<string>? EffectsCarried { get; set; }

    [JsonPropertyName("effects_worn")]
    public List<string>? EffectsWorn { get; set; }

    [JsonPropertyName("effects_wielded")]
    public List<string>? EffectsWielded { get; set; }

    [JsonPropertyName("effects_activated")]
    public List<string>? EffectsActivated { get; set; }
}
