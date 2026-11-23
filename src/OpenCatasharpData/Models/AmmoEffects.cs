using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>Projectile field effects spawned along a trail or at the impact location.</summary>
public sealed class CddaProjectileField
{
    [JsonPropertyName("field_type")]
    public string? FieldType { get; set; }

    [JsonPropertyName("intensity_min")]
    public decimal? MinimumIntensity { get; set; }

    [JsonPropertyName("intensity_max")]
    public decimal? MaximumIntensity { get; set; }

    [JsonPropertyName("radius")]
    public decimal? Radius { get; set; }

    [JsonPropertyName("radius_z")]
    public decimal? VerticalRadius { get; set; }

    [JsonPropertyName("chance")]
    public decimal? Chance { get; set; }

    [JsonPropertyName("size")]
    public decimal? Size { get; set; }

    [JsonPropertyName("check_passable")]
    public bool? CheckPassable { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Projectile effect applied on direct hit or to creatures in an area.</summary>
public sealed class CddaProjectileStatusEffect
{
    [JsonPropertyName("effect")]
    public string? Effect { get; set; }

    [JsonPropertyName("duration")]
    public CddaDuration? Duration { get; set; }

    [JsonPropertyName("intensity")]
    public decimal? Intensity { get; set; }

    [JsonPropertyName("intensity_min")]
    public decimal? MinimumIntensity { get; set; }

    [JsonPropertyName("intensity_max")]
    public decimal? MaximumIntensity { get; set; }

    [JsonPropertyName("radius")]
    public decimal? Radius { get; set; }

    [JsonPropertyName("chance")]
    public decimal? Chance { get; set; }

    [JsonPropertyName("all_bp")]
    public bool? AllBodyParts { get; set; }

    [JsonPropertyName("need_touch_skin")]
    public bool? NeedTouchSkin { get; set; }

    [JsonPropertyName("hits_amount")]
    public CddaScalarRange? HitsAmount { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Spell triggered by an ammunition effect.</summary>
public sealed class CddaAmmoSpellReference
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>
/// Standalone ammo_effect definition, referenced by IDs in AMMO and GUNMOD.
/// Unlike AmmoData, this model is not a subtype of ITEM.
/// </summary>
public sealed class AmmoEffectDefinition : CddaDefinition
{
    [JsonPropertyName("trigger_chance")]
    public decimal? TriggerChance { get; set; }

    [JsonPropertyName("aoe")]
    public List<CddaProjectileField>? AreaOfEffect { get; set; }

    [JsonPropertyName("trail")]
    public List<CddaProjectileField>? Trail { get; set; }

    [JsonPropertyName("explosion")]
    public CddaExplosion? Explosion { get; set; }

    [JsonPropertyName("on_hit_effects")]
    public List<CddaProjectileStatusEffect>? OnHitEffects { get; set; }

    [JsonPropertyName("aoe_effects")]
    public List<CddaProjectileStatusEffect>? AreaStatusEffects { get; set; }

    [JsonPropertyName("do_flashbang")]
    public bool? DoFlashbang { get; set; }

    [JsonPropertyName("do_emp_blast")]
    public bool? DoElectromagneticBlast { get; set; }

    [JsonPropertyName("always_cast_spell")]
    public bool? AlwaysCastSpell { get; set; }

    [JsonPropertyName("spell_data")]
    public List<CddaAmmoSpellReference>? SpellData { get; set; }

    [JsonPropertyName("eoc")]
    [JsonConverter(typeof(CddaOneOrManyJsonConverter<CddaEffectReference>))]
    public List<CddaEffectReference>? EffectsOnCondition { get; set; }
}
