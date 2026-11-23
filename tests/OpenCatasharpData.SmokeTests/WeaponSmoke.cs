using System.Text.Json;
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

internal static class WeaponSmoke
{
    public static void Run()
    {
        var gun = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {
              "type":"ITEM","id":"fixture_gun","subtypes":["GUN"],
              "skill":"pistol","ammo":["9mm","40"],"ranged_damage":0,
              "range":4,"dispersion":480,"sight_dispersion":60,
              "recoil":120,"min_strength":8,"min_cycle_recoil":100,
              "durability":8,"gun_jam_mult":1.25,"blackpowder_tolerance":56,
              "clip_size":15,"handling":10,"energy_drain":"2 kJ",
              "heat_per_shot":10,"cooling_value":3,"overheat_threshold":100,
              "ammo_to_fire":2,
              "firing_requirements":{
                "DEFAULT":[{"pocket":"ammo","qty":1},{"pocket":"battery","qty":5,"extra":true}]
              },
              "modes":[["DEFAULT","single",1],["VOLLEY","volley",3,["VOLLEY"]]],
              "reload":200,"reload_noise":"Ping!","reload_noise_volume":4,
              "built_in_mods":["integrated_suppressor"],
              "default_mods":["red_dot_sight"],
              "barrel_volume":"30 mL","barrel_length":"102 mm",
              "valid_mod_locations":[["brass catcher",1],["grip",2]],
              "loudness":10
            }
            """);
        var gunData = gun.GunData;
        Check(gunData is not null &&
            gunData.Skill == "pistol" &&
            gunData.Ammo?.SequenceEqual(["9mm", "40"]) == true &&
            gunData.Range == 4m &&
            gunData.Dispersion == 480m &&
            gunData.SightDispersion == 60m &&
            gunData.Recoil == 120m &&
            gunData.MinimumStrength == 8 &&
            gunData.MinimumCycleRecoil == 100m &&
            gunData.Durability == 8 &&
            gunData.GunJamMultiplier == 1.25m &&
            gunData.BlackpowderTolerance == 56m,
            "Gun accuracy, recoil, skills and reliability");
        Check(gunData.ClipSize == 15 && gunData.Handling == 10 &&
            gunData.EnergyDrain?.Text == "2 kJ" &&
            gunData.HeatPerShot == 10m &&
            gunData.CoolingValue == 3m &&
            gunData.OverheatThreshold == 100m &&
            gunData.AmmoToFire == 2,
            "Gun energy, heat and charge consumption");
        Check(gunData.FiringRequirements?["DEFAULT"].Count == 2 &&
            gunData.FiringRequirements["DEFAULT"][1].Pocket == "battery" &&
            gunData.FiringRequirements["DEFAULT"][1].Quantity == 5 &&
            gunData.FiringRequirements["DEFAULT"][1].AdditionalProperties?["extra"].GetBoolean() == true,
            "Multimag firing requirements include nested typed costs");
        Check(gunData.Modes?.Count == 2 &&
            gunData.Modes[0].Id == "DEFAULT" &&
            gunData.Modes[1].Flags?.SequenceEqual(["VOLLEY"]) == true,
            "Firing modes can carry optional arrays of flags");
        Check(JsonSerializer.Serialize(gunData.Modes![1]).Contains("[\"VOLLEY\"]"),
            "Array-form mode flags round-trip");
        Check(gunData.ReloadTime == 200 &&
            gunData.ReloadNoise?.Singular == "Ping!" &&
            gunData.ReloadNoiseVolume == 4m &&
            gunData.BuiltInMods?.Single() == "integrated_suppressor" &&
            gunData.DefaultMods?.Single() == "red_dot_sight" &&
            gunData.BarrelVolume?.Millilitres == 30m &&
            gunData.BarrelLength?.Millimetres == 102m &&
            gunData.ValidModLocations?[1].Amount == 2m &&
            gunData.Loudness == 10m,
            "Gun reload, barrel and integrated mods");
        Check(ReferenceEquals(gun.RangedDamage, gunData.RangedDamage) &&
            gunData.RangedDamage?[0].Amount == 0m &&
            gunData.RangedDamage[0].WasNumber &&
            JsonSerializer.Serialize(gun.RangedDamage) == "0",
            "Legacy numeric ranged damage remains typed and round-trippable");

        var ammo = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {
              "type":"ITEM","id":"fixture_ammo","subtypes":["AMMO"],
              "ammo_type":"223","count":30,"stack_size":47,
              "damage":{"damage_type":"bullet","amount":39,"armor_penetration":2,
                        "barrels":[{"barrel_length":"28 mm","amount":13}]},
              "shot_damage":{"damage_type":"bullet","amount":10},
              "thrown_damage":{"damage_type":"bash","amount":7},
              "projectile_count":3,"dispersion":20,
              "dispersion_modifier":[{"barrel_length":"337 mm","dispersion":30}],
              "shot_spread":100,"shot_counter":5,
              "critical_multiplier":4,"recovery_chance":95,
              "show_stats":true,"effects":["COOKOFF"],
              "recoil":1500,"loudness":10,"range":5,
              "casing":"223_casing","explode_in_fire":true,
              "explosion":{"power":42,"distance_factor":0.8,"fire":false,
                           "shrapnel":{"casing_mass":10,"fragment_mass":1.5,"recovery":25,
                                       "drop":"steel_fragment"},
                           "custom_explosion":true}
            }
            """);
        var ammoData = ammo.AmmoData;
        Check(ammoData is not null && ammoData.AmmoTypes?.Single() == "223" &&
            ammoData.Damage?.DamageType == "bullet" &&
            ammoData.Damage.Amount == 39m &&
            ammoData.Damage.ArmorPenetration == 2m &&
            ammoData.Damage.Barrels?[0].BarrelLength?.Millimetres == 28m &&
            ammoData.ShotDamage?.Amount == 10m &&
            ammoData.ThrownDamage?.Amount == 7m,
            "Ammo damage, shot damage and barrel curves");
        Check(ammoData.ProjectileCount == 3 &&
            ammoData.Dispersion == 20m &&
            ammoData.DispersionModifier?.ByBarrel?[0].BarrelLength?.Millimetres == 337m &&
            ammoData.DispersionModifier.ByBarrel[0].Dispersion == 30m &&
            ammoData.ShotSpread == 100m &&
            ammoData.ShotCounter == 5 &&
            ammoData.CriticalMultiplier == 4m &&
            ammoData.RecoveryChance == 95m &&
            ammoData.ShowStats == true,
            "Projectile spread, critical damage and ammunition recovery");
        Check(ammoData.Effects?.Single() == "COOKOFF" &&
            ammoData.Recoil == 1500m &&
            ammoData.Loudness == 10m &&
            ammoData.Range == 5m &&
            ammoData.Count == 30 &&
            ammoData.StackSize == 47 &&
            ammoData.Casing == "223_casing",
            "Ammo consumption and performance values");
        Check(ammoData.ExplodeInFire == true &&
            ammoData.Explosion?.Power == 42m &&
            ammoData.Explosion.Shrapnel?.FragmentMass == 1.5m &&
            ammoData.Explosion.AdditionalProperties?["custom_explosion"].GetBoolean() == true,
            "Explosion and optional shrapnel details");
        Check(JsonSerializer.Serialize(ammoData.DispersionModifier).Contains("337 mm"),
            "Barrel dispersion shape can be serialized");

        var flatDispersion = JsonSerializer.Deserialize<CddaAmmoDispersion>("20");
        Check(flatDispersion?.Flat == 20m &&
            JsonSerializer.Serialize(flatDispersion) == "20",
            "Scalar ammo dispersion modifier round trips");

        var magazine = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"fixture_magazine","subtypes":["MAGAZINE"],
             "ammo_type":["40","357sig"],"capacity":15,"count":2,
             "default_ammo":"556","reload_time":100,
             "mag_jam_mult":1.25,"linkage":"ammolink"}
            """);
        var mag = magazine.MagazineData;
        Check(mag?.AmmoTypes?.SequenceEqual(["40","357sig"]) == true &&
            mag.Capacity == 15 && mag.Count == 2 &&
            mag.DefaultAmmo == "556" && mag.ReloadTime == 100 &&
            mag.MagazineJamMultiplier == 1.25m &&
            mag.Linkage == "ammolink",
            "Magazine ammo types, loading, linkages and reliability");

        var modifier = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {
              "type":"ITEM","id":"fixture_gunmod","subtypes":["GUNMOD"],
              "location":"stock","mod_targets":["crossbow"],
              "install_time":"30 s","barrel_length":"45 mm",
              "acceptable_ammo":["9mm"],"ammo_modifier":["57"],
              "magazine_adaptor":[["223",["stanag30","stanag40"]]],
              "pocket_mods":[{"pocket_type":"MAGAZINE_WELL",
                              "item_restriction":["ai_338mag"]}],
              "consumption_mods":[{"pocket":"power","multiply":0.5},
                                  {"pocket":"ammo","set":2}],
              "capacity_mods":[{"pocket":"ammo","multiply":1.5}],
              "damage_modifier":{"damage_type":"bullet","amount":-1},
              "dispersion_modifier":15,"handling_modifier":4,
              "loudness_modifier":4,"loudness_multiplier":0.5,
              "range_modifier":2,"range_multiplier":1.2,
              "integral_longest_side":"5 cm",
              "overwrite_min_cycle_recoil":1350,
              "reload_noise":"chuk chuk.","reload_noise_volume":2,
              "aim_speed_modifier":-2,"add_mod":[["grip",1],["sights",1]],
              "energy_drain_multiplier":1.2,"energy_drains_multiplier":2.5,
              "field_of_view":270,"shot_spread_multiplier_modifier":-0.8,
              "energy_drain_modifier":"200 kJ","reload_modifier":-10,
              "min_str_required_mod":14,"aim_speed":3,
              "ammo_effects":["BEANBAG"],"consume_chance":5000,
              "consume_divisor":10,
              "mode_modifier":[["AUTO","auto",4,["VOLLEY"]]],
              "hide_modes":["DEFAULT"],
              "mode_firing_requirements":{
                "AUTO":[{"pocket":"ammo","qty":3},{"pocket":"power","qty":5}]},
              "overheat_threshold_modifier":100,
              "overheat_threshold_multiplier":1.5,
              "cooling_value_modifier":2,"cooling_value_multiplier":0.5,
              "heat_per_shot_modifier":-2,"heat_per_shot_multiplier":2,
              "is_bayonet":true,"is_visible_when_installed":false,
              "blacklist_slot":["rail","underbarrel"],
              "blacklist_mod":["m203","m320"],"to_hit_mod":-1
            }
            """);
        var mod = modifier.GunModData;
        Check(mod is not null &&
            mod.Location == "stock" && mod.ModTargets?.Single() == "crossbow" &&
            mod.InstallTime?.Seconds == 30m && mod.BarrelLength?.Millimetres == 45m &&
            mod.AcceptableAmmo?.Single() == "9mm" &&
            mod.AmmoModifier?.Single() == "57",
            "Gunmod installation and ammo compatibility");
        Check(mod.MagazineAdaptors?[0].AmmoType == "223" &&
            mod.MagazineAdaptors[0].Magazines.SequenceEqual(["stanag30","stanag40"]) &&
            mod.PocketMods?[0].ItemRestriction?.Single() == "ai_338mag" &&
            mod.ConsumptionMods?[0].Multiply == 0.5m &&
            mod.ConsumptionMods[1].Set == 2 &&
            mod.CapacityMods?[0].Multiply == 1.5m,
            "Magazine adapter tuples and pocket-cost/capacity modifiers");
        Check(JsonSerializer.Serialize(mod.MagazineAdaptors).Contains("stanag40"),
            "Magazine adapter tuple round trips");
        Check(mod.DamageModifier?.Amount == -1 &&
            mod.DispersionModifier == 15 &&
            mod.HandlingModifier == 4 &&
            mod.LoudnessModifier == 4 &&
            mod.LoudnessMultiplier == 0.5m &&
            mod.RangeModifier == 2 &&
            mod.RangeMultiplier == 1.2m &&
            mod.IntegralLongestSide?.Centimetres == 5m &&
            mod.OverwriteMinimumCycleRecoil == 1350m,
            "Gunmod ballistic and ergonomic modifiers");
        Check(mod.ReloadNoise?.Singular == "chuk chuk." &&
            mod.ReloadNoiseVolume == 2m &&
            mod.AimSpeedModifier == -2m &&
            mod.AddMod?.Count == 2 &&
            mod.AddMod[1].Name == "sights" &&
            mod.EnergyDrainMultiplier == 1.2m &&
            mod.EnergyDrainsMultiplier == 2.5m &&
            mod.EnergyDrainModifier?.Text == "200 kJ" &&
            mod.FieldOfView == 270m &&
            mod.ShotSpreadMultiplierModifier == -0.8m &&
            mod.ReloadModifier == -10m,
            "Aiming, mods, energy and reload adjustments");
        Check(mod.MinimumStrengthModifier == 14 &&
            mod.AimSpeed == 3m &&
            mod.AmmoEffects?.Single() == "BEANBAG" &&
            mod.ConsumeChance == 5000m &&
            mod.ConsumeDivisor == 10m &&
            mod.ModeModifier?[0].Flags?.Single() == "VOLLEY" &&
            mod.HideModes?.Single() == "DEFAULT" &&
            mod.ModeFiringRequirements?["AUTO"][1].Quantity == 5,
            "Installed-mode changes and firing requirements");
        Check(mod.OverheatThresholdModifier == 100m &&
            mod.OverheatThresholdMultiplier == 1.5m &&
            mod.CoolingValueModifier == 2m &&
            mod.CoolingValueMultiplier == 0.5m &&
            mod.HeatPerShotModifier == -2m &&
            mod.HeatPerShotMultiplier == 2m &&
            mod.IsBayonet == true &&
            mod.VisibleWhenInstalled == false &&
            mod.BlacklistSlots?.Count == 2 &&
            mod.BlacklistMods?.Count == 2 &&
            mod.ToHitModifier == -1m,
            "Gunmod heat model and compatibility blacklists");

        var mixed = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"multi_gun","subtypes":["GUN","MAGAZINE","AMMO"],
             "ammo_type":["223"],"ammo":["223"],"capacity":20,"damage":{"damage_type":"bullet","amount":10},
             "recoil":100,"firing_requirements":{"DEFAULT":[{"pocket":"ammo","qty":1}]}}
            """);
        Check(mixed.GunData?.Recoil == 100m &&
            mixed.AmmoData?.Recoil == 100m &&
            mixed.AmmoData?.Damage?.Amount == 10m &&
            mixed.MagazineData?.Capacity == 20 &&
            mixed.MagazineData.AmmoTypes?.Single() == "223",
            "Combined weapon types retain all components independently");

        var fault = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"faulty_gun","subtypes":["GUN"],
             "faults":[{"fault":"fault_stovepipe","weight":20},
                       {"fault_group":"handles","weight_override":20,
                        "weight_add":-5,"weight_mult":0.8}]}
            """);
        Check(fault.Faults?.Count == 2 &&
            fault.Faults[0].Weight == 20m &&
            fault.Faults[1].WeightOverride == 20m &&
            fault.Faults[1].WeightAdd == -5m &&
            fault.Faults[1].WeightMultiplier == 0.8m,
            "Fault probabilities and group modifiers are typed");

        Console.WriteLine("Expanded gun, ammo, magazine and gunmod smoke tests passed.");
    }

    public static async Task RunPinnedForkAsync(string root)
    {
        var ammoPath = Path.Combine(root, "ammo", "223.json");
        var gunPath = Path.Combine(root, "gun", "9mm.json");
        var magPath = Path.Combine(root, "magazine", "9mm.json");
        var modPath = Path.Combine(root, "gunmod", "accessories.json");
        if (!(File.Exists(ammoPath) && File.Exists(gunPath) &&
              File.Exists(magPath) && File.Exists(modPath)))
            throw new InvalidOperationException("Pinned weapon files missing.");

        var ammo = await CddaJsonLoader.LoadFileAsync(ammoPath);
        var gun = await CddaJsonLoader.LoadFileAsync(gunPath);
        var mags = await CddaJsonLoader.LoadFileAsync(magPath);
        var mods = await CddaJsonLoader.LoadFileAsync(modPath);

        var bullet = ammo.OfType<ItemDefinition>().First(item => item.IdString == "223");
        Check(bullet.AmmoData?.Damage?.Amount == 39m &&
            bullet.AmmoData?.DispersionModifier?.ByBarrel?.Count > 0 &&
            bullet.AmmoData.DispersionModifier.ByBarrel[0].BarrelLength?.Millimetres > 0,
            "Real .223 ammunition barrel dispersion is eagerly typed");

        var glock = gun.OfType<ItemDefinition>().First(item => item.IdString == "glock_19");
        Check(glock.GunData?.Dispersion == 480m &&
            glock.GunData.BarrelLength?.Millimetres == 102m,
            "Real Glock item has eagerly typed gun geometry and accuracy");

        Check(mags.OfType<ItemDefinition>().Any(item =>
                item.MagazineData is not null && item.AmmoTypes?.Count > 0),
            "Real magazine file contains typed ammunition compatibility");

        Check(mods.OfType<ItemDefinition>().Any(item =>
                item.GunModData?.InstallTime is not null),
            "Real gunmods have typed installation times");

        Console.WriteLine("Pinned CDDA weapon JSON fixtures passed.");
    }

    private static void Check(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException("FAILED: " + label);
    }
}
