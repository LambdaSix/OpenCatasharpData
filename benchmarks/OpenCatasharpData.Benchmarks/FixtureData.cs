namespace OpenCatasharpData.Benchmarks;

/// <summary>
/// Deterministic, representative CDDA fixtures. No network checkout or
/// random data generation is involved in a timed benchmark.
/// </summary>
internal static class FixtureData
{
    public const string Generic = """
        {
          "type":"ITEM","id":"hammer","name":{"str":"claw hammer"},
          "description":"A sturdy hammer.","weight":"920 g","volume":"750 ml",
          "price":"14 USD","material":["steel","wood"],
          "flags":["SHEATH_SWORD"],"qualities":[["HAMMER",3],["PRY",1]],
          "melee_damage":{"bash":13}
        }
        """;

    public const string Comestible = """
        {
          "type":"ITEM","id":"apple_cider","subtypes":["COMESTIBLE"],
          "name":{"str_sp":"apple cider"},"comestible_type":"DRINK",
          "description":"Fresh apple cider.","weight":"263 g","volume":"250 ml",
          "calories":120,"quench":65,"fun":2,"spoils_in":"6 days",
          "material":[{"type":"water","portion":100},{"type":"fruit","portion":1}],
          "vitamins":[["vitC",167],["calcium",1]],
          "flags":["EATEN_COLD"]
        }
        """;

    public const string Armor = """
        {
          "type":"ITEM","id":"boots","subtypes":["ARMOR"],
          "name":{"str_sp":"boots (pair)"},"description":"Leather boots.",
          "weight":"1060 g","volume":"2500 ml","price":"100 USD",
          "warmth":20,"flags":["VARSIZE","WATERPROOF"],
          "armor":[
            {"covers":["foot_l","foot_r"],"coverage":90,"encumbrance":[5,10],
             "material":[{"type":"leather","covered_by_mat":100,"thickness":2.5}]},
            {"covers":["foot_l","foot_r"],"coverage":100,"encumbrance":10}
          ],
          "pocket_data":[{"pocket_type":"CONTAINER","rigid":false,
                          "max_contains_volume":"100 ml","max_contains_weight":"250 g"}]
        }
        """;

    /// <summary>Detailed armour fixture kept separate from the original baseline.</summary>
    public const string ArmorDetailed = """
        {
          "type":"ITEM","id":"armoured_pack","subtypes":["ARMOR","TOOL"],
          "name":{"str":"armoured pack"},"weight":"2 kg","volume":"3 L",
          "warmth":10,"material_thickness":2.5,"sided":true,
          "max_worn":1,"valid_mods":["steel_padded"],
          "environmental_protection":4,
          "armor":[
            {
              "covers":["torso"],"coverage":95,"cover_melee":80,
              "cover_ranged":70,"cover_vitals":20,
              "encumbrance":[5,15],"layers":["OUTER"],
              "breathability":"GOOD","rigid_layer_only":true,
              "volume_encumber_modifier":0.25,
              "activity_noise":{"volume":8,"chance":60},
              "material":[{"type":"kevlar","covered_by_mat":100,"thickness":2}]
            }
          ],
          "pocket_data":[
            {
              "pocket_type":"CONTAINER","max_contains_volume":"2 L",
              "max_contains_weight":"5 kg","max_item_length":"70 cm",
              "min_item_length":"5 cm","min_item_volume":"250 ml",
              "default_magazine":"heavy_battery_cell",
              "open_container":true,"airtight":true,
              "extra_encumbrance":2,"ripoff":3,
              "sealed_data":{"spoil_multiplier":0.5},
              "activity_noise":{"volume":3,"chance":20}
            }
          ],
          "charges_per_use":1
        }
        """;

    public const string Gun = """
        {
          "type":"ITEM","id":"glock_19","subtypes":["GUN"],
          "name":{"str":"Glock carry pistol"},"copy-from":"gun_base_9mm_semi_pistol",
          "description":"Compact 9x19mm pistol.","weight":"600 g","volume":"386 ml",
          "price":"690 USD","material":["plastic","steel"],
          "skill":"pistol","dispersion":480,"handling":10,"durability":7,
          "variants":[{"id":"glock_19","weight":100},
                      {"id":"glock_19_cop","weight":0}],
          "pocket_data":[{"pocket_type":"MAGAZINE_WELL","rigid":true,
                          "ammo_restriction":{"9mm":15}}]
        }
        """;

    public const string Ammo = """
        {
          "type":"ITEM","id":"9mm","subtypes":["AMMO"],
          "name":{"str_sp":"9x19mm JHP"},"weight":"12 g","volume":"115 ml",
          "count":50,"stack_size":50,"ammo_type":"9mm","casing":"9mm_casing",
          "range":14,"damage":{"damage_type":"bullet","amount":23},
          "material":["brass","lead","powder"]
        }
        """;

    public const string Magazine = """
        {
          "type":"ITEM","id":"glockmag","subtypes":["MAGAZINE"],
          "name":{"str":"Glock magazine"},"weight":"70 g","volume":"76 ml",
          "ammo_type":["9mm"],"reload_time":140,
          "pocket_data":[{"pocket_type":"MAGAZINE","rigid":true,
                          "ammo_restriction":{"9mm":15}}]
        }
        """;

    public const string Tool = """
        {
          "type":"ITEM","id":"butchering_kit","subtypes":["TOOL"],
          "name":{"str":"butchering kit"},"weight":"1170 g","volume":"800 ml",
          "copy-from":"base_kitchen_knife","charges_per_use":1,
          "qualities":[["CUT",2],["CUT_FINE",1],["BUTCHER",37]]
        }
        """;

    public const string Book = """
        {
          "type":"ITEM","id":"book_fict_tpl","subtypes":["BOOK"],
          "name":{"str":"Generic Fiction Book"},"chapters":16,
          "intelligence":5,"read_fun":2,"time":"15 m"
        }
        """;

    public const string GunMod = """
        {
          "type":"ITEM","id":"arrowrest","subtypes":["GUNMOD"],
          "name":{"str":"arrow rest"},"weight":"16 g","volume":"250 ml",
          "location":"arrow rest","mod_targets":["bow"],
          "dispersion_modifier":-20,"install_time":"15 m"
        }
        """;

    /// <summary>Deep weapon DTO workloads; legacy benchmark fixtures stay unchanged.</summary>
    public const string GunDetailed = """
        {
          "type":"ITEM","id":"detailed_gun","subtypes":["GUN"],
          "skill":"pistol","ammo":["9mm"],"dispersion":480,
          "recoil":130,"sight_dispersion":60,"min_cycle_recoil":140,
          "gun_jam_mult":1.25,"heat_per_shot":10,
          "energy_drain":"2 kJ","barrel_volume":"30 mL",
          "firing_requirements":{"DEFAULT":[{"pocket":"ammo","qty":1},
                                            {"pocket":"battery","qty":5}]},
          "modes":[["DEFAULT","semi-auto",1],["VOLLEY","volley",3,["VOLLEY"]]],
          "valid_mod_locations":[["barrel",1],["sights",1]],
          "barrel_length":"102 mm","ranged_damage":0
        }
        """;

    public const string AmmoDetailed = """
        {
          "type":"ITEM","id":"detailed_ammo","subtypes":["AMMO"],
          "ammo_type":"223","count":30,"stack_size":47,
          "damage":{"damage_type":"bullet","amount":39,"armor_penetration":2,
                    "barrels":[{"barrel_length":"28 mm","amount":13}]},
          "dispersion_modifier":[{"barrel_length":"337 mm","dispersion":30}],
          "shot_damage":{"damage_type":"bullet","amount":10},
          "projectile_count":2,"recovery_chance":75,
          "explosion":{"power":20,"shrapnel":0}
        }
        """;

    public const string GunModDetailed = """
        {
          "type":"ITEM","id":"detailed_gunmod","subtypes":["GUNMOD"],
          "location":"stock","mod_targets":["crossbow"],
          "install_time":"30 s",
          "magazine_adaptor":[["223",["stanag30","stanag40"]]],
          "consumption_mods":[{"pocket":"power","multiply":0.5}],
          "capacity_mods":[{"pocket":"ammo","multiply":1.5}],
          "mode_modifier":[["AUTO","auto",4,["VOLLEY"]]],
          "mode_firing_requirements":{"AUTO":[{"pocket":"ammo","qty":3}]},
          "overheat_threshold_modifier":10,"is_bayonet":true
        }
        """;

    public const string AmmoEffect = """
        {
          "type":"ammo_effect","id":"detailed_effect","trigger_chance":25,
          "aoe":[{"field_type":"fd_fog","intensity_min":1,"radius":5}],
          "trail":[{"field_type":"fd_smoke","intensity_min":1,"chance":50}],
          "explosion":{"power":200,"shrapnel":{"casing_mass":10,"fragment_mass":0.5}}
        }
        """;

    public static readonly string[] Records =
        [Generic, Comestible, Armor, Gun, Ammo, Magazine, Tool, Book, GunMod];

    public static readonly string BatchJson = "[" + string.Join(",", Records) + "]";
    public static readonly byte[] BatchUtf8 = System.Text.Encoding.UTF8.GetBytes(BatchJson);
}
