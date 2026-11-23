using System.Text.Json;
using OpenCatasharpData;
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

internal static class AmmoEffectSmoke
{
    public static void Run()
    {
        var parsed = CddaRecordParser.Default.Parse(
            """
            {
              "type":"ammo_effect","id":"TEST_EXPLOSIVE","trigger_chance":20,
              "aoe":[{"field_type":"fd_fog","intensity_min":1,"intensity_max":3,
                      "radius":5,"radius_z":1,"chance":100,"size":0,
                      "check_passable":false,"custom_field":"retained"}],
              "trail":[{"field_type":"fd_smoke","intensity_min":1,"chance":50}],
              "explosion":{"power":240,"distance_factor":0.8,"fire":false,
                           "shrapnel":{"casing_mass":300,"fragment_mass":0.0025,
                                       "recovery":25,"drop":"steel_fragment"}},
              "on_hit_effects":[{"effect":"bile_stink","duration":"5 m",
                                 "intensity":1,"need_touch_skin":true}],
              "aoe_effects":[{"effect":"onfire","duration":"1 m",
                              "intensity_min":1,"chance":50,"hits_amount":[1,3]}],
              "do_flashbang":true,"do_emp_blast":false,"always_cast_spell":true,
              "spell_data":[{"id":"spell_plasma_fan"}],
              "eoc":["EOC_random_mutate",{"id":"EOC_inline","effect":{"u_message":"text"}}],
              "unknown_ammo_effect_flag":true
            }
            """);
        var effect = parsed as AmmoEffectDefinition;
        Check(effect is not null && effect.IdString == "TEST_EXPLOSIVE" &&
            effect.TriggerChance == 20m &&
            effect.AreaOfEffect?[0].FieldType == "fd_fog" &&
            effect.AreaOfEffect[0].Radius == 5m &&
            effect.AreaOfEffect[0].VerticalRadius == 1m &&
            effect.AreaOfEffect[0].CheckPassable == false &&
            effect.AreaOfEffect[0].AdditionalProperties?["custom_field"].GetString() == "retained",
            "Typed ammo_effect definitions, fields and extension keys");
        Check(effect.Trail?[0].FieldType == "fd_smoke" &&
            effect.Explosion?.Power == 240m &&
            effect.Explosion.Shrapnel?.FragmentMass == 0.0025m &&
            effect.OnHitEffects?[0].Duration?.Minutes == 5m &&
            effect.OnHitEffects[0].NeedTouchSkin == true &&
            effect.AreaStatusEffects?[0].HitsAmount?.Maximum == 3m,
            "Projectile trails, explosion shrapnel, hit and area effects");
        Check(effect.DoFlashbang == true && effect.DoElectromagneticBlast == false &&
            effect.AlwaysCastSpell == true && effect.SpellData?[0].Id == "spell_plasma_fan" &&
            effect.EffectsOnCondition?.Count == 2 &&
            effect.EffectsOnCondition[0].Id == "EOC_random_mutate" &&
            effect.EffectsOnCondition[1].Id == "EOC_inline" &&
            effect.AdditionalProperties?["unknown_ammo_effect_flag"].GetBoolean() == true,
            "Auxiliary effects, spells and linked EOCs remain typed");

        var numericShrapnel = (AmmoEffectDefinition)CddaRecordParser.Default.Parse(
            """{"type":"ammo_effect","id":"LEGACY","explosion":{"power":20,"shrapnel":0}}""");
        Check(numericShrapnel.Explosion?.Shrapnel?.LegacyValue == 0m,
            "Legacy numeric shrapnel accepted");
        using var roundtrip = JsonDocument.Parse(JsonSerializer.Serialize(numericShrapnel));
        Check(roundtrip.RootElement.GetProperty("explosion")
            .GetProperty("shrapnel").GetDecimal() == 0m,
            "Numeric shrapnel JSON representation preserved");

        Console.WriteLine("Standalone ammunition effect DTO smoke tests passed.");
    }

    public static async Task RunPinnedForkAsync(string itemDirectory)
    {
        var path = Path.GetFullPath(Path.Combine(itemDirectory, "..", "ammo_effects.json"));
        if (!File.Exists(path))
            throw new FileNotFoundException("Pinned ammunition effects file missing.", path);
        var effects = await CddaJsonLoader.LoadFileAsync(path);
        Check(effects.Count == 89 &&
            effects.All(e => e is AmmoEffectDefinition),
            "All pinned core ammunition effect definitions eagerly deserialise");
        Check(effects.OfType<AmmoEffectDefinition>().Any(e =>
            e.Explosion?.Shrapnel is not null),
            "Real ammunition effects include typed shrapnel data");
        Console.WriteLine($"Pinned ammo_effect records parsed: {effects.Count}");
    }

    private static void Check(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException("FAILED: " + label);
    }
}
