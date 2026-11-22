using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>Typed common properties shared by all CDDA ITEM definitions.</summary>
public class ItemDefinition : CddaDefinition
{
    [JsonPropertyName("subtypes")]
    public List<string>? Subtypes { get; set; }

    [JsonPropertyName("name")]
    public CddaText? Name { get; set; }

    [JsonPropertyName("description")]
    public CddaText? Description { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("weight")]
    public CddaMass? Weight { get; set; }

    [JsonPropertyName("volume")]
    public CddaVolume? Volume { get; set; }

    [JsonPropertyName("price")]
    public CddaScalar? Price { get; set; }

    [JsonPropertyName("price_postapoc")]
    public CddaScalar? PostApocalypticPrice { get; set; }

    [JsonPropertyName("material")]
    [JsonConverter(typeof(CddaMaterialListConverter))]
    public List<CddaMaterialPart>? Material { get; set; }

    [JsonPropertyName("flags")]
    public List<string>? Flags { get; set; }

    [JsonPropertyName("qualities")]
    [JsonConverter(typeof(CddaQualitiesJsonConverter))]
    public List<CddaQuality>? Qualities { get; set; }

    // Shared so composite items retain all equipment facets as typed values.
    [JsonPropertyName("armor")]
    public List<CddaArmorSection>? Armor { get; set; }

    [JsonPropertyName("pocket_data")]
    public List<CddaPocket>? PocketData { get; set; }

    // Common properties intentionally belong to the base class: in CDDA an
    // ITEM may have multiple subtypes, or no declared subtype at all.
    [JsonPropertyName("looks_like")]
    public string? LooksLike { get; set; }

    [JsonPropertyName("longest_side")]
    public CddaLength? LongestSide { get; set; }

    [JsonPropertyName("barrel_length")]
    public CddaLength? BarrelLength { get; set; }

    [JsonPropertyName("display_type")]
    public string? DisplayType { get; set; }

    [JsonPropertyName("ascii_picture")]
    public string? AsciiPicture { get; set; }

    [JsonPropertyName("phase")]
    public string? Phase { get; set; }

    [JsonPropertyName("variant_type")]
    public string? VariantType { get; set; }

    [JsonPropertyName("variants")]
    public List<CddaItemVariant>? Variants { get; set; }

    [JsonPropertyName("melee_damage")]
    public CddaMeleeDamage? MeleeDamage { get; set; }

    [JsonPropertyName("to_hit")]
    public CddaToHit? ToHit { get; set; }

    [JsonPropertyName("use_action")]
    [JsonConverter(typeof(CddaUseActionsJsonConverter))]
    public List<CddaUseAction>? UseActions { get; set; }

    [JsonPropertyName("ranged_damage")]
    [JsonConverter(typeof(CddaDamageListJsonConverter))]
    public List<CddaDamage>? RangedDamage { get; set; }

    [JsonPropertyName("weapon_category")]
    public List<string>? WeaponCategories { get; set; }

    [JsonPropertyName("conditional_names")]
    public List<CddaConditionalName>? ConditionalNames { get; set; }

    [JsonPropertyName("faults")]
    public List<CddaFault>? Faults { get; set; }

    [JsonPropertyName("tool_ammo")]
    [JsonConverter(typeof(CddaStringListConverter))]
    public List<string>? ToolAmmo { get; set; }

    [JsonPropertyName("charges")]
    public int? Charges { get; set; }

    [JsonPropertyName("environmental_protection")]
    public decimal? EnvironmentalProtection { get; set; }

    [JsonPropertyName("replace_materials")]
    public Dictionary<string, string>? ReplaceMaterials { get; set; }

    [JsonPropertyName("techniques")]
    public List<string>? Techniques { get; set; }

    [JsonPropertyName("revert_to")]
    public string? RevertTo { get; set; }

    [JsonPropertyName("tick_action")]
    [JsonConverter(typeof(CddaUseActionsJsonConverter))]
    public List<CddaUseAction>? TickActions { get; set; }

    [JsonPropertyName("drop_action")]
    [JsonConverter(typeof(CddaUseActionsJsonConverter))]
    public List<CddaUseAction>? DropActions { get; set; }

    [JsonPropertyName("integral_volume")]
    public CddaVolume? IntegralVolume { get; set; }

    [JsonPropertyName("integral_weight")]
    public CddaMass? IntegralWeight { get; set; }

    [JsonPropertyName("min_skills")]
    [JsonConverter(typeof(CddaNamedAmountsConverter))]
    public List<CddaNamedAmount>? MinimumSkills { get; set; }

    [JsonPropertyName("sealed")]
    public bool? Sealed { get; set; }

    [JsonPropertyName("ammo_type")]
    [JsonConverter(typeof(CddaStringListConverter))]
    public List<string>? AmmoTypes { get; set; }
}

public sealed class GenericItemDefinition : ItemDefinition
{
}

public sealed class ArmorItemDefinition : ItemDefinition
{
    [JsonPropertyName("warmth")]
    public int? Warmth { get; set; }

    [JsonPropertyName("material_thickness")]
    public CddaScalar? MaterialThickness { get; set; }
}

public sealed class ComestibleItemDefinition : ItemDefinition
{
    [JsonPropertyName("comestible_type")]
    public string? ComestibleType { get; set; }

    [JsonPropertyName("calories")]
    public decimal? Calories { get; set; }

    [JsonPropertyName("quench")]
    public int? Quench { get; set; }

    [JsonPropertyName("fun")]
    public int? Fun { get; set; }

    [JsonPropertyName("spoils_in")]
    public CddaDuration? SpoilsIn { get; set; }

    [JsonPropertyName("container")]
    public string? Container { get; set; }

    [JsonPropertyName("vitamins")]
    [JsonConverter(typeof(CddaVitaminsJsonConverter))]
    public List<CddaVitamin>? Vitamins { get; set; }
}

public sealed class ToolItemDefinition : ItemDefinition
{
    [JsonPropertyName("charges_per_use")]
    public int? ChargesPerUse { get; set; }

    [JsonPropertyName("max_charges")]
    public int? MaxCharges { get; set; }

    [JsonPropertyName("initial_charges")]
    public int? InitialCharges { get; set; }

    [JsonPropertyName("turns_per_charge")]
    public int? TurnsPerCharge { get; set; }
}

public sealed class GunItemDefinition : ItemDefinition
{
    [JsonPropertyName("skill")]
    public string? Skill { get; set; }

    [JsonPropertyName("dispersion")]
    public int? Dispersion { get; set; }

    [JsonPropertyName("durability")]
    public int? Durability { get; set; }

    [JsonPropertyName("reload")]
    public int? ReloadTime { get; set; }

    [JsonPropertyName("handling")]
    public int? Handling { get; set; }

    [JsonPropertyName("modes")]
    public List<CddaGunMode>? Modes { get; set; }

    [JsonPropertyName("valid_mod_locations")]
    [JsonConverter(typeof(CddaNamedAmountsConverter))]
    public List<CddaNamedAmount>? ValidModLocations { get; set; }

    [JsonPropertyName("clip_size")]
    public int? ClipSize { get; set; }

    [JsonPropertyName("ammo")]
    [JsonConverter(typeof(CddaStringListConverter))]
    public List<string>? Ammo { get; set; }
}

public sealed class AmmoItemDefinition : ItemDefinition
{
    [JsonPropertyName("damage")]
    public CddaDamage? Damage { get; set; }

    [JsonPropertyName("shot_damage")]
    public CddaDamage? ShotDamage { get; set; }

    [JsonPropertyName("projectile_count")]
    public int? ProjectileCount { get; set; }

    [JsonPropertyName("dispersion")]
    public int? Dispersion { get; set; }

    [JsonPropertyName("effects")]
    public List<string>? Effects { get; set; }

    [JsonPropertyName("recoil")]
    public decimal? Recoil { get; set; }

    [JsonPropertyName("loudness")]
    public decimal? Loudness { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("stack_size")]
    public int? StackSize { get; set; }

    [JsonPropertyName("range")]
    public int? Range { get; set; }

    [JsonPropertyName("casing")]
    public string? Casing { get; set; }
}

public sealed class MagazineItemDefinition : ItemDefinition
{
    [JsonPropertyName("capacity")]
    public int? Capacity { get; set; }

    [JsonPropertyName("reload_time")]
    public int? ReloadTime { get; set; }
}

public sealed class BookItemDefinition : ItemDefinition
{
    [JsonPropertyName("read_skill")]
    public string? ReadSkill { get; set; }

    [JsonPropertyName("skill")]
    public string? Skill { get; set; }

    [JsonPropertyName("required_level")]
    public int? RequiredLevel { get; set; }

    [JsonPropertyName("max_level")]
    public int? MaxLevel { get; set; }

    [JsonPropertyName("intelligence")]
    public int? Intelligence { get; set; }

    [JsonPropertyName("chapters")]
    public int? Chapters { get; set; }

    [JsonPropertyName("read_fun")]
    public int? ReadFun { get; set; }

    [JsonPropertyName("time")]
    public CddaDuration? ReadingTime { get; set; }
}

public sealed class GunModItemDefinition : ItemDefinition
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("mod_targets")]
    public List<string>? ModTargets { get; set; }

    [JsonPropertyName("dispersion_modifier")]
    public int? DispersionModifier { get; set; }

    [JsonPropertyName("handling_modifier")]
    public int? HandlingModifier { get; set; }

    [JsonPropertyName("install_time")]
    public CddaDuration? InstallTime { get; set; }
}
