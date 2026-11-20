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
    public CddaScalar? Weight { get; set; }

    [JsonPropertyName("volume")]
    public CddaScalar? Volume { get; set; }

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
    [JsonConverter(typeof(CddaNamedAmountsConverter))]
    public List<CddaNamedAmount>? Qualities { get; set; }

    // Shared so composite items retain all equipment facets as typed values.
    [JsonPropertyName("armor")]
    public List<CddaArmorSection>? Armor { get; set; }

    [JsonPropertyName("pocket_data")]
    public List<CddaPocket>? PocketData { get; set; }

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
    public CddaScalar? SpoilsIn { get; set; }

    [JsonPropertyName("container")]
    public string? Container { get; set; }

    [JsonPropertyName("vitamins")]
    [JsonConverter(typeof(CddaNamedAmountsConverter))]
    public List<CddaNamedAmount>? Vitamins { get; set; }
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

    [JsonPropertyName("ammo")]
    [JsonConverter(typeof(CddaStringListConverter))]
    public List<string>? Ammo { get; set; }
}

public sealed class AmmoItemDefinition : ItemDefinition
{
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
    public CddaScalar? ReadingTime { get; set; }
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
    public CddaScalar? InstallTime { get; set; }
}
