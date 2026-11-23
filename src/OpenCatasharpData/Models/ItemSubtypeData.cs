using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>
/// Armor-only fields of an ITEM. Missing values remain null to distinguish
/// an omitted source property from a game-applied default.
/// Sections references the eagerly deserialized ITEM armor array.
/// </summary>
public sealed class ArmorData
{
    [JsonIgnore]
    public List<CddaArmorSection>? Sections { get; internal set; }

    [JsonPropertyName("warmth")]
    public int? Warmth { get; set; }

    [JsonPropertyName("material_thickness")]
    public CddaScalar? MaterialThickness { get; set; }

    [JsonPropertyName("sided")]
    public bool? Sided { get; set; }

    [JsonPropertyName("power_armor")]
    public bool? PowerArmor { get; set; }

    [JsonPropertyName("max_worn")]
    public int? MaximumWorn { get; set; }

    [JsonPropertyName("valid_mods")]
    public List<string>? ValidMods { get; set; }

    [JsonPropertyName("non_functional")]
    public string? NonFunctionalItem { get; set; }

    [JsonPropertyName("damage_verb")]
    public string? DamageVerb { get; set; }

    [JsonPropertyName("environmental_protection")]
    public decimal? EnvironmentalProtection { get; set; }

    [JsonPropertyName("encumbrance")]
    public CddaScalarRange? Encumbrance { get; set; }

    [JsonPropertyName("max_encumbrance")]
    public decimal? MaximumEncumbrance { get; set; }

    [JsonPropertyName("covers")]
    [JsonConverter(typeof(CddaStringListConverter))]
    public List<string>? Covers { get; set; }

    [JsonPropertyName("coverage")]
    public decimal? Coverage { get; set; }

    [JsonPropertyName("cover_melee")]
    public decimal? MeleeCoverage { get; set; }

    [JsonPropertyName("cover_ranged")]
    public decimal? RangedCoverage { get; set; }

    [JsonPropertyName("cover_vitals")]
    public decimal? VitalCoverage { get; set; }

    [JsonPropertyName("energy_shield_max_hp")]
    public System.Text.Json.JsonElement? EnergyShieldMaximumHitPoints { get; set; }
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

/// <summary>Gun-specific statistics and firing/energy requirements.</summary>
public sealed class GunData
{
    [JsonIgnore]
    public List<CddaDamage>? RangedDamage { get; internal set; }

    [JsonIgnore]
    public CddaLength? BarrelLength { get; internal set; }

    [JsonPropertyName("skill")]
    public string? Skill { get; set; }

    [JsonPropertyName("ammo")]
    [JsonConverter(typeof(CddaStringListConverter))]
    public List<string>? Ammo { get; set; }

    [JsonPropertyName("range")]
    public decimal? Range { get; set; }

    [JsonPropertyName("dispersion")]
    public decimal? Dispersion { get; set; }

    [JsonPropertyName("sight_dispersion")]
    public decimal? SightDispersion { get; set; }

    [JsonPropertyName("recoil")]
    public decimal? Recoil { get; set; }

    [JsonPropertyName("min_strength")]
    public int? MinimumStrength { get; set; }

    [JsonPropertyName("min_cycle_recoil")]
    public decimal? MinimumCycleRecoil { get; set; }

    [JsonPropertyName("durability")]
    public int? Durability { get; set; }

    [JsonPropertyName("gun_jam_mult")]
    public decimal? GunJamMultiplier { get; set; }

    [JsonPropertyName("blackpowder_tolerance")]
    public decimal? BlackpowderTolerance { get; set; }

    [JsonPropertyName("clip_size")]
    public int? ClipSize { get; set; }

    [JsonPropertyName("handling")]
    public decimal? Handling { get; set; }

    [JsonPropertyName("energy_drain")]
    public CddaScalar? EnergyDrain { get; set; }

    [JsonPropertyName("heat_per_shot")]
    public decimal? HeatPerShot { get; set; }

    [JsonPropertyName("cooling_value")]
    public decimal? CoolingValue { get; set; }

    [JsonPropertyName("overheat_threshold")]
    public decimal? OverheatThreshold { get; set; }

    [JsonPropertyName("ammo_to_fire")]
    public int? AmmoToFire { get; set; }

    [JsonPropertyName("firing_requirements")]
    public Dictionary<string, List<CddaFiringCost>>? FiringRequirements { get; set; }

    [JsonPropertyName("modes")]
    public List<CddaGunMode>? Modes { get; set; }

    [JsonPropertyName("reload")]
    public int? ReloadTime { get; set; }

    [JsonPropertyName("reload_noise")]
    public CddaText? ReloadNoise { get; set; }

    [JsonPropertyName("reload_noise_volume")]
    public decimal? ReloadNoiseVolume { get; set; }

    [JsonPropertyName("ammo_effects")]
    public List<string>? AmmoEffects { get; set; }

    [JsonPropertyName("built_in_mods")]
    public List<string>? BuiltInMods { get; set; }

    [JsonPropertyName("default_mods")]
    public List<string>? DefaultMods { get; set; }

    [JsonPropertyName("barrel_volume")]
    public CddaVolume? BarrelVolume { get; set; }

    [JsonPropertyName("valid_mod_locations")]
    [JsonConverter(typeof(CddaNamedAmountsConverter))]
    public List<CddaNamedAmount>? ValidModLocations { get; set; }

    [JsonPropertyName("loudness")]
    public decimal? Loudness { get; set; }
}

/// <summary>Ammunition performance, damage and projectile statistics.</summary>
public sealed class AmmoData
{
    [JsonIgnore]
    public List<string>? AmmoTypes { get; internal set; }

    [JsonPropertyName("damage")]
    public CddaDamage? Damage { get; set; }

    [JsonPropertyName("shot_damage")]
    public CddaDamage? ShotDamage { get; set; }

    [JsonIgnore]
    public List<CddaDamage>? ThrownDamage { get; internal set; }

    [JsonPropertyName("projectile_count")]
    public int? ProjectileCount { get; set; }

    [JsonPropertyName("dispersion")]
    public decimal? Dispersion { get; set; }

    [JsonPropertyName("dispersion_modifier")]
    public CddaAmmoDispersion? DispersionModifier { get; set; }

    [JsonPropertyName("shot_spread")]
    public decimal? ShotSpread { get; set; }

    [JsonPropertyName("shot_counter")]
    public int? ShotCounter { get; set; }

    [JsonPropertyName("critical_multiplier")]
    public decimal? CriticalMultiplier { get; set; }

    [JsonPropertyName("recovery_chance")]
    public decimal? RecoveryChance { get; set; }

    [JsonPropertyName("show_stats")]
    public bool? ShowStats { get; set; }

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
    public decimal? Range { get; set; }

    [JsonPropertyName("casing")]
    public string? Casing { get; set; }

    [JsonIgnore]
    public bool? ExplodeInFire { get; internal set; }

    [JsonIgnore]
    public CddaExplosion? Explosion { get; internal set; }
}

/// <summary>Magazine capacity, default loading and jam characteristics.</summary>
public sealed class MagazineData
{
    [JsonIgnore]
    public List<string>? AmmoTypes { get; internal set; }

    [JsonPropertyName("capacity")]
    public int? Capacity { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("default_ammo")]
    public string? DefaultAmmo { get; set; }

    [JsonPropertyName("reload_time")]
    public int? ReloadTime { get; set; }

    [JsonPropertyName("mag_jam_mult")]
    public decimal? MagazineJamMultiplier { get; set; }

    [JsonPropertyName("linkage")]
    public string? Linkage { get; set; }
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

/// <summary>Weapon attachment modifiers, compatibility rules, and firing-mode changes.</summary>
public sealed class GunModData
{
    [JsonIgnore]
    public CddaLength? BarrelLength { get; internal set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("mod_targets")]
    public List<string>? ModTargets { get; set; }

    [JsonPropertyName("install_time")]
    public CddaDuration? InstallTime { get; set; }

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

    [JsonPropertyName("damage_modifier")]
    public CddaDamage? DamageModifier { get; set; }

    [JsonPropertyName("dispersion_modifier")]
    public decimal? DispersionModifier { get; set; }

    [JsonPropertyName("loudness_modifier")]
    public decimal? LoudnessModifier { get; set; }

    [JsonPropertyName("loudness_multiplier")]
    public decimal? LoudnessMultiplier { get; set; }

    [JsonPropertyName("range_modifier")]
    public decimal? RangeModifier { get; set; }

    [JsonPropertyName("range_multiplier")]
    public decimal? RangeMultiplier { get; set; }

    [JsonPropertyName("integral_longest_side")]
    public CddaLength? IntegralLongestSide { get; set; }

    [JsonPropertyName("overwrite_min_cycle_recoil")]
    public decimal? OverwriteMinimumCycleRecoil { get; set; }

    [JsonPropertyName("reload_noise")]
    public CddaText? ReloadNoise { get; set; }

    [JsonPropertyName("reload_noise_volume")]
    public decimal? ReloadNoiseVolume { get; set; }

    [JsonPropertyName("aim_speed_modifier")]
    public decimal? AimSpeedModifier { get; set; }

    [JsonPropertyName("add_mod")]
    [JsonConverter(typeof(CddaNamedAmountsConverter))]
    public List<CddaNamedAmount>? AddMod { get; set; }

    [JsonPropertyName("energy_drain_multiplier")]
    public decimal? EnergyDrainMultiplier { get; set; }

    [JsonPropertyName("energy_drains_multiplier")]
    public decimal? EnergyDrainsMultiplier { get; set; }

    [JsonPropertyName("field_of_view")]
    public decimal? FieldOfView { get; set; }

    [JsonPropertyName("shot_spread_multiplier_modifier")]
    public decimal? ShotSpreadMultiplierModifier { get; set; }

    [JsonPropertyName("energy_drain_modifier")]
    public CddaScalar? EnergyDrainModifier { get; set; }

    [JsonPropertyName("reload_modifier")]
    public decimal? ReloadModifier { get; set; }

    [JsonPropertyName("min_str_required_mod")]
    public int? MinimumStrengthModifier { get; set; }

    [JsonPropertyName("aim_speed")]
    public decimal? AimSpeed { get; set; }

    [JsonPropertyName("ammo_effects")]
    public List<string>? AmmoEffects { get; set; }

    [JsonPropertyName("consume_chance")]
    public decimal? ConsumeChance { get; set; }

    [JsonPropertyName("consume_divisor")]
    public decimal? ConsumeDivisor { get; set; }

    [JsonPropertyName("handling_modifier")]
    public decimal? HandlingModifier { get; set; }

    [JsonPropertyName("mode_modifier")]
    public List<CddaGunMode>? ModeModifier { get; set; }

    [JsonPropertyName("hide_modes")]
    public List<string>? HideModes { get; set; }

    [JsonPropertyName("mode_firing_requirements")]
    public Dictionary<string, List<CddaFiringCost>>? ModeFiringRequirements { get; set; }

    [JsonPropertyName("overheat_threshold_modifier")]
    public decimal? OverheatThresholdModifier { get; set; }

    [JsonPropertyName("overheat_threshold_multiplier")]
    public decimal? OverheatThresholdMultiplier { get; set; }

    [JsonPropertyName("cooling_value_modifier")]
    public decimal? CoolingValueModifier { get; set; }

    [JsonPropertyName("cooling_value_multiplier")]
    public decimal? CoolingValueMultiplier { get; set; }

    [JsonPropertyName("heat_per_shot_modifier")]
    public decimal? HeatPerShotModifier { get; set; }

    [JsonPropertyName("heat_per_shot_multiplier")]
    public decimal? HeatPerShotMultiplier { get; set; }

    [JsonPropertyName("is_bayonet")]
    public bool? IsBayonet { get; set; }

    [JsonPropertyName("is_visible_when_installed")]
    public bool? VisibleWhenInstalled { get; set; }

    [JsonPropertyName("blacklist_slot")]
    [JsonConverter(typeof(CddaStringListConverter))]
    public List<string>? BlacklistSlots { get; set; }

    [JsonPropertyName("blacklist_mod")]
    [JsonConverter(typeof(CddaStringListConverter))]
    public List<string>? BlacklistMods { get; set; }

    [JsonPropertyName("to_hit_mod")]
    public decimal? ToHitModifier { get; set; }
}
