using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

public sealed class ArmorData
{
    [JsonIgnore]
    public List<CddaArmorSection>? Sections { get; internal set; }

    [JsonPropertyName("warmth")]
    public int? Warmth { get; set; }

    [JsonPropertyName("material_thickness")]
    public CddaScalar? MaterialThickness { get; set; }
}

public sealed class ComestibleData
{
    [JsonPropertyName("stim")]
    public decimal? Stimulation { get; set; }

    [JsonPropertyName("sleepiness_mod")]
    public decimal? SleepinessModifier { get; set; }

    [JsonPropertyName("healthy")]
    public decimal? Health { get; set; }

    [JsonPropertyName("addiction_potential")]
    public int? AddictionPotential { get; set; }

    [JsonPropertyName("addiction_type")]
    [JsonConverter(typeof(CddaOneOrManyJsonConverter<CddaAddiction>))]
    public List<CddaAddiction>? Addictions { get; set; }

    [JsonPropertyName("consumption_effect_on_conditions")]
    [JsonConverter(typeof(CddaOneOrManyJsonConverter<CddaEffectReference>))]
    public List<CddaEffectReference>? ConsumptionEffects { get; set; }

    [JsonPropertyName("monotony_penalty")]
    public int? MonotonyPenalty { get; set; }

    [JsonPropertyName("stack_size")]
    public int? StackSize { get; set; }

    [JsonPropertyName("tool")]
    public string? RequiredTool { get; set; }

    [JsonPropertyName("seasonings")]
    public List<string>? Seasonings { get; set; }

    [JsonPropertyName("freezing_point")]
    public decimal? FreezingPointCelsius { get; set; }

    [JsonPropertyName("cooks_like")]
    public string? CooksLike { get; set; }

    [JsonPropertyName("eats_like")]
    public string? EatsLike { get; set; }

    [JsonPropertyName("parasites")]
    public int? Parasites { get; set; }

    [JsonPropertyName("contamination")]
    public List<CddaContamination>? Contamination { get; set; }

    [JsonPropertyName("rot_spawn")]
    public CddaRotSpawn? RotSpawn { get; set; }

    [JsonPropertyName("smoking_result")]
    public string? SmokingResult { get; set; }

    [JsonPropertyName("petfood")]
    public List<string>? PetFood { get; set; }

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

public sealed class ToolData
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

public sealed class GunData
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

public sealed class AmmoData
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

public sealed class MagazineData
{
    [JsonPropertyName("capacity")]
    public int? Capacity { get; set; }

    [JsonPropertyName("reload_time")]
    public int? ReloadTime { get; set; }
}

public sealed class BookData
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

public sealed class GunModData
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
