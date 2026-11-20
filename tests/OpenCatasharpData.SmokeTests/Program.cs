using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenCatasharpData;
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

// Reduced, representative records drawn from LambdaSix/Cataclysm-DDA.
const string sample = """
[
  {
    "type":"skill", "id":"speech", "name":{"str":"social"},
    "description":"Persuasion and bartering.",
    "level_descriptions_theory":[{"level":0,"description":"Newcomer."}]
  },
  {
    "type":"material", "abstract":"polymer", "name":"Polymer",
    "density":1.25, "copy-from":"base_material", "relative":{"chip_resist":2}
  },
  {
    "type":"ITEM_CATEGORY", "id":"software", "name_header":{"str":"Software"},
    "name_noun":{"str_sp":"software"}
  },
  {
    "type":"ITEM", "id":"fire", "name":{"str_sp":"nearby fire"},
    "material":["wood","steel"], "weight":5, "custom_attribute":{"enabled":true}
  },
  {
    "type":"ITEM", "id":"bastsandals", "subtypes":["ARMOR"],
    "name":{"str_sp":"cord sandals (pair)"}, "weight":"68 g",
    "armor":[{"covers":["foot_l","foot_r"],"coverage":40,"encumbrance":[5,10],
              "material":[{"type":"cotton","covered_by_mat":100,"thickness":2}]}]
  },
  {
    "type":"ITEM", "id":"apple_cider", "subtypes":["COMESTIBLE"],
    "name":{"str_sp":"apple cider"}, "comestible_type":"DRINK",
    "calories":120, "quench":65, "fun":2, "spoils_in":"6 days",
    "material":[{"type":"water","portion":100},{"type":"fruit","portion":1}],
    "vitamins":[["vitC",167],["calcium",1]]
  },
  {
    "type":"ITEM", "id":"butchering_kit", "subtypes":["TOOL"],
    "name":{"str":"butchering kit"}, "copy-from":"base_kitchen_knife",
    "qualities":[["CUT",2],["BUTCHER",37]], "charges_per_use":1
  },
  {
    "type":"ITEM", "id":"glock_19", "subtypes":["GUN"],
    "name":{"str":"Glock carry pistol"}, "dispersion":480, "handling":10,
    "variants":[{"id":"glock_19_cop","weight":5}]
  },
  {
    "type":"ITEM", "id":"9mm", "subtypes":["AMMO"],
    "name":{"str_sp":"9x19mm JHP"}, "ammo_type":"9mm",
    "count":50, "stack_size":50, "casing":"9mm_casing",
    "damage":{"damage_type":"bullet","amount":23}
  },
  {
    "type":"ITEM", "id":"glockmag", "subtypes":["MAGAZINE"],
    "name":{"str":"Glock magazine"}, "ammo_type":["9mm"],
    "pocket_data":[{"pocket_type":"MAGAZINE","rigid":true,
                    "ammo_restriction":{"9mm":15}}], "reload_time":140
  },
  {
    "type":"ITEM", "abstract":"book_fict_tpl", "subtypes":["BOOK"],
    "name":{"str":"Generic Fiction Book"}, "chapters":16,
    "intelligence":5, "time":"15 m"
  },
  {
    "type":"ITEM", "id":"arrowrest", "subtypes":["GUNMOD"],
    "name":{"str":"arrow rest"}, "location":"arrow rest",
    "mod_targets":["bow"], "dispersion_modifier":-20, "install_time":"15 m"
  },
  {
    "type":"ITEM", "abstract":"magazine_belt", "subtypes":["ARMOR","MAGAZINE"],
    "name":{"str":"ammo belt"}, "ammo_type":["9mm"],
    "armor":[{"covers":["torso"],"coverage":10}],
    "pocket_data":[{"pocket_type":"MAGAZINE","ammo_restriction":{"9mm":30}}]
  },
  {
    "type":"ITEM", "id":"fermented_drink", "subtypes":["BREWABLE","COMESTIBLE"],
    "comestible_type":"DRINK", "quench":5
  },
  {
    "type":"other_type", "id":["first","second"], "special":42
  }
]
""";

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(sample));
var definitions = await CddaJsonLoader.LoadAsync(stream);

Check(definitions.Count == 15, "Definition count");
var skill = Expect<SkillDefinition>(definitions[0], "Skill");
Check(skill.IdString == "speech" && skill.Name?.Singular == "social", "Eager skill name and ID");
Check(skill.Description?.Singular == "Persuasion and bartering.", "String translation");
Check(skill.LevelDescriptionsTheory?[0].Level == 0, "Nested typed skill DTO");

var material = Expect<MaterialDefinition>(definitions[1], "Material");
Check(material.Name?.Singular == "Polymer", "Material DTO");
Check(material.CopyFrom == "base_material", "Inheritance metadata");
Check(material.Relative?.ValueKind == JsonValueKind.Object, "Relative patch preservation");

var category = Expect<ItemCategoryDefinition>(definitions[2], "Category");
Check(category.NameNoun?.Singular == "software", "str_sp translation");

var generic = Expect<GenericItemDefinition>(definitions[3], "Generic item");
Check(generic.IdString == "fire", "Item ID");
Check(generic.Weight?.Number == 5m, "Numeric item weight");
Check(generic.Material?.Select(m => m.Type).SequenceEqual(["wood", "steel"]) == true,
    "Materials eagerly typed");
Check(generic.AdditionalProperties?["custom_attribute"].GetProperty("enabled").GetBoolean() == true,
    "Unmodelled fields preserved");

var armor = Expect<ArmorItemDefinition>(definitions[4], "Armor");
Check(armor.Weight?.Text == "68 g", "Unit-bearing weight");
Check(armor.Armor?[0].Covers?.Count == 2, "Armor coverage is a typed list");
Check(armor.Armor?[0].Encumbrance?.Maximum == 10m, "Armor encumbrance range");
Check(armor.Armor?[0].Material?[0].Thickness == 2m, "Armor material thickness");

var food = Expect<ComestibleItemDefinition>(definitions[5], "Comestible");
Check(food.Calories == 120m && food.Quench == 65 && food.Fun == 2, "Nutrition fields");
Check(food.Material?[0].Portion == 100m, "Material portions");
Check(food.Vitamins?[0] == new CddaNamedAmount("vitC", 167m), "Vitamin pairs");

var tool = Expect<ToolItemDefinition>(definitions[6], "Tool");
Check(tool.Qualities?[1] == new CddaNamedAmount("BUTCHER", 37m), "Tool quality pairs");

var gun = Expect<GunItemDefinition>(definitions[7], "Gun");
Check(gun.Dispersion == 480 && gun.Handling == 10, "Gun statistics");
Check(gun.Variants?[0].Id == "glock_19_cop" && gun.Variants[0].Weight == 5m,
    "Gun variants eagerly typed");

var ammo = Expect<AmmoItemDefinition>(definitions[8], "Ammo");
Check(ammo.AmmoTypes?.SequenceEqual(["9mm"]) == true, "String ammo type");
Check(ammo.Count == 50 && ammo.Casing == "9mm_casing", "Ammo fields");
Check(ammo.Damage?.DamageType == "bullet" && ammo.Damage.Amount == 23m,
    "Ranged ammunition damage eagerly typed");

var magazine = Expect<MagazineItemDefinition>(definitions[9], "Magazine");
Check(magazine.AmmoTypes?.SequenceEqual(["9mm"]) == true, "Array ammo type");
Check(magazine.PocketData?[0].AmmoRestriction?["9mm"] == 15, "Typed pocket data");

var book = Expect<BookItemDefinition>(definitions[10], "Book");
Check(book.ReadingTime?.Text == "15 m" && book.Chapters == 16, "Book fields");

var gunmod = Expect<GunModItemDefinition>(definitions[11], "Gun mod");
Check(gunmod.ModTargets?.SequenceEqual(["bow"]) == true, "Gun mod targets");

var combined = Expect<ArmorItemDefinition>(definitions[12], "Combined item");
Check(combined.Subtypes?.Count == 2, "Multiple subtypes retained");
Check(combined.AmmoTypes?[0] == "9mm" && combined.PocketData?[0].AmmoRestriction?["9mm"] == 30,
    "Combined item's equipment facets preserved");

var brewable = Expect<ComestibleItemDefinition>(definitions[13], "Unknown then known subtype");
Check(brewable.Quench == 5, "First recognised subtype selected");

var unknown = Expect<UnknownDefinition>(definitions[14], "Unknown");
Check(unknown.IdString is null && unknown.AdditionalProperties?["special"].GetInt32() == 42,
    "Unknown and non-string IDs preserved");

var single = CddaRecordParser.Default.Parse(
    """{"type":"ITEM","subtypes":["COMESTIBLE"],"id":"single","calories":250}""");
Check(single is ComestibleItemDefinition { Calories: 250m, IdString: "single" },
    "Single-record JSON parser");
var encoded = Encoding.UTF8.GetBytes(
    """{"type":"ITEM","subtypes":["MAGAZINE"],"id":"single_mag","ammo_type":["battery"]}""");
Check(CddaRecordParser.Default.Parse(encoded) is MagazineItemDefinition,
    "Single-record UTF-8 parser");

var custom = new CddaRecordParser(
[
    new JsonDefinitionHandler<CustomDefinition>(element =>
        CddaDefaultHandlers.TypeIs(element, "custom")),
    .. CddaDefaultHandlers.Create()
]);
var customResult = custom.Parse("""{"type":"custom","id":"mine","power":8}""");
Check(customResult is CustomDefinition { Power: 8, IdString: "mine" },
    "Independent custom parser handler");

// Exercise every common-item converter, including CDDA's multiple JSON shapes.
var fullItem = Expect<GenericItemDefinition>(CddaRecordParser.Default.Parse(
    """
    {
      "type":"ITEM","id":"sample_melee",
      "looks_like":"hammer","longest_side":"20 cm",
      "display_type":"BY_WEIGHT","phase":"solid","ascii_picture":"hammer",
      "variant_type":"generic",
      "variants":[{"id":"melee_a","name":{"str":"Hammer A"},
                   "description":"A variant","weight":3,"append":true,
                   "color":"red","expand_snippets":false}],
      "melee_damage":{"bash":12,"cut":2},
      "to_hit":{"grip":"solid","length":"hand","surface":"line","balance":"neutral"},
      "use_action":[{"type":"transform","target":"hammer_on","menu_text":"Activate",
                     "msg":"It starts.","moves":100},
                    "HEAT_LIQUID_ITEMS"],
      "weapon_category":["HAMMERS"],
      "conditional_names":[{"type":"COMPONENT_ID","condition":"steel",
                            "name":{"str":"steel hammer"}}],
      "ranged_damage":{"damage_type":"bullet","amount":0}
    }
    """), "Generic complex fields");
Check(fullItem.LooksLike == "hammer" && fullItem.LongestSide?.Text == "20 cm",
    "Common item display and dimensions");
Check(fullItem.MeleeDamage?.Bash == 12m && fullItem.MeleeDamage.Cut == 2m,
    "Typed melee damage");
Check(fullItem.ToHit?.Grip == "solid" && fullItem.ToHit.Balance == "neutral",
    "Typed to_hit object");
Check(fullItem.UseActions?.Count == 2 &&
    fullItem.UseActions[0].Target == "hammer_on" &&
    fullItem.UseActions[0].Message?.Singular == "It starts." &&
    fullItem.UseActions[1].Type == "HEAT_LIQUID_ITEMS" &&
    fullItem.UseActions[1].WasString,
    "Typed mixed string/object use actions");
Check(fullItem.Variants?[0].Name?.Singular == "Hammer A" &&
    fullItem.Variants[0].Append == true, "Typed cosmetic variants");
Check(fullItem.RangedDamage?[0].DamageType == "bullet" &&
    fullItem.ConditionalNames?[0].Name?.Singular == "steel hammer",
    "Typed common conditionals and ranged damage");

var legacyHit = Expect<GenericItemDefinition>(CddaRecordParser.Default.Parse(
    """{"type":"ITEM","id":"legacy","to_hit":1,"use_action":{"type":"holster"}}"""),
    "Legacy to_hit");
Check(legacyHit.ToHit?.LegacyBonus == 1m &&
    legacyHit.UseActions?.Single().Type == "holster",
    "Legacy to_hit and single-object use_action");

var ammoDetails = Expect<AmmoItemDefinition>(CddaRecordParser.Default.Parse(
    """
    {
      "type":"ITEM","id":"bullet","subtypes":["AMMO"],
      "damage":{"damage_type":"bullet","amount":28,"armor_penetration":4,
                "barrels":[{"barrel_length":"25 mm","amount":18}]},
      "shot_damage":{"damage_type":"bullet","amount":10},
      "projectile_count":3
    }
    """), "Detailed ammo");
Check(ammoDetails.Damage?.Barrels?[0].BarrelLength?.Text == "25 mm" &&
    ammoDetails.Damage.Barrels[0].Amount == 18m,
    "Ammo barrel-dependent damage eagerly typed");
Check(ammoDetails.ShotDamage?.Amount == 10m &&
    ammoDetails.ProjectileCount == 3, "Shot damage typed");

var detailedGun = Expect<GunItemDefinition>(CddaRecordParser.Default.Parse(
    """
    {
      "type":"ITEM","id":"pistol","subtypes":["GUN"],
      "modes":[["DEFAULT","semi-auto",1],["AUTO","auto",4,"NPC_AVOID"]],
      "valid_mod_locations":[["barrel",1],["sights",1]],
      "clip_size":6,
      "ranged_damage":[{"damage_type":"bullet","amount":2},
                       {"damage_type":"stab","amount":1}]
    }
    """), "Detailed gun");
Check(detailedGun.Modes?.Count == 2 &&
    detailedGun.Modes[1].Flag == "NPC_AVOID" &&
    detailedGun.Modes[1].Shots == 4, "Typed gun mode tuples");
Check(detailedGun.ValidModLocations?[0] == new CddaNamedAmount("barrel", 1m) &&
    detailedGun.RangedDamage?.Count == 2, "Gun mod slot tuples and ranged damage list");

var detailedArmor = Expect<ArmorItemDefinition>(CddaRecordParser.Default.Parse(
    """
    {
      "type":"ITEM","id":"coat","subtypes":["ARMOR"],
      "armor":[{"covers":["torso"],"layers":["BELTED"],"cover_vitals":70,
                 "encumbrance_modifiers":["NONE"]}],
      "pocket_data":[{"pocket_type":"CONTAINER","max_item_length":"30 cm",
                      "moves":100,"item_restriction":["candle"],
                      "flag_restriction":["BATTERY_LIGHT"],
                      "magazine_well":"250 ml","volume_encumber_modifier":1.25,
                      "holster":true,"transparent":true}]
    }
    """), "Detailed armor");
Check(detailedArmor.Armor?[0].Layers?[0] == "BELTED" &&
    detailedArmor.Armor[0].CoverVitals == 70m, "Armor layers and vitals");
Check(detailedArmor.PocketData?[0].MaxItemLength?.Text == "30 cm" &&
    detailedArmor.PocketData[0].Moves == 100 &&
    detailedArmor.PocketData[0].ItemRestriction?[0] == "candle" &&
    detailedArmor.PocketData[0].MagazineWell?.Text == "250 ml",
    "Typed pocket geometry and restrictions");

var additionalFields = Expect<GenericItemDefinition>(CddaRecordParser.Default.Parse(
    """
    {
      "type":"ITEM","id":"additional_fields",
      "replace_materials":{"steel":"budget_steel_chain"},
      "techniques":["WBLOCK_1"],
      "revert_to":"off",
      "tick_action":["MP3_ON"],
      "drop_action":{"type":"transform","target":"off"},
      "integral_volume":"50 ml","integral_weight":"2 g",
      "min_skills":[["fabrication",2]],
      "sealed":true
    }
    """), "Additional common fields");
Check(additionalFields.ReplaceMaterials?["steel"] == "budget_steel_chain" &&
    additionalFields.Techniques?[0] == "WBLOCK_1" &&
    additionalFields.RevertTo == "off", "Eager replacements and techniques");
Check(additionalFields.TickActions?[0].Type == "MP3_ON" &&
    additionalFields.DropActions?[0].Target == "off" &&
    additionalFields.IntegralVolume?.Text == "50 ml" &&
    additionalFields.MinimumSkills?[0].Amount == 2m,
    "Extra actions, dimensions, and skill tuples");

var directory = Path.Combine(Path.GetTempPath(), "OpenCatasharpData-" + Guid.NewGuid());
Directory.CreateDirectory(Path.Combine(directory, "nested"));
try
{
    await File.WriteAllTextAsync(Path.Combine(directory, "nested", "sample.json"), sample);
    await File.WriteAllTextAsync(Path.Combine(directory, "other.json"),
        """[{"type":"ITEM","id":"fire","name":"second definition"}]""");

    var catalog = await CddaJsonLoader.LoadDirectoryCatalogAsync(directory);
    Check(catalog.Entries.Count == 16, "Catalog materializes all records");
    Check(catalog.Get<ComestibleItemDefinition>().Count == 2, "Typed catalog query");
    Check(catalog.FindById("fire").Count == 2, "Repeated IDs preserved");
    Check(catalog.FindById("glock_19")[0].FilePath.EndsWith("sample.json"),
        "Catalog source filename");

    var streamedCount = 0;
    await foreach (var entry in CddaJsonLoader.LoadDirectoryAsync(directory))
    {
        Check(entry.FilePath.EndsWith(".json", StringComparison.Ordinal), "Source filenames");
        streamedCount++;
    }
    Check(streamedCount == 16, "Streaming directory loader");

    await using var input = File.OpenRead(Path.Combine(directory, "other.json"));
    var customBatch = await CddaJsonLoader.LoadAsync(input, parser: custom);
    Check(customBatch[0] is GenericItemDefinition, "Custom handler registry works with loader");
}
finally
{
    Directory.Delete(directory, recursive: true);
}

Console.WriteLine("OpenCatasharpData smoke tests passed.");

if (args.Length == 2 && args[0] == "--scan-dir")
{
    var scanDirectory = Path.GetFullPath(args[1]);
    var scannedFiles = 0;
    var scannedDefinitions = 0;
    var failures = 0;
    foreach (var file in Directory.EnumerateFiles(scanDirectory, "*.json",
                 SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
    {
        try
        {
            var records = await CddaJsonLoader.LoadFileAsync(file);
            scannedFiles++;
            scannedDefinitions += records.Count;
        }
        catch (Exception exception)
        {
            failures++;
            if (failures <= 40)
                Console.Error.WriteLine($"{Path.GetRelativePath(scanDirectory, file)}: " +
                                        $"{exception.GetType().Name}: {exception.Message}");
        }
    }
    Console.WriteLine($"Full-fork probe: {scannedFiles} successfully parsed files, " +
                      $"{scannedDefinitions} records, {failures} failed files.");
    if (failures != 0) Environment.ExitCode = 1;
}


static T Expect<T>(CddaDefinition value, string label) where T : CddaDefinition
{
    Check(value is T, $"Expected {label} but got {value.GetType().Name}");
    return (T)value;
}

static void Check(bool success, string label)
{
    if (!success) throw new InvalidOperationException($"FAILED: {label}");
}

public sealed class CustomDefinition : CddaDefinition
{
    [JsonPropertyName("power")]
    public int Power { get; set; }
}
