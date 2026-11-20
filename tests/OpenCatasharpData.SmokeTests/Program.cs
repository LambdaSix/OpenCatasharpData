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
Check(gun.AdditionalProperties?["variants"].ValueKind == JsonValueKind.Array,
    "Gun variants preserved");

var ammo = Expect<AmmoItemDefinition>(definitions[8], "Ammo");
Check(ammo.AmmoTypes?.SequenceEqual(["9mm"]) == true, "String ammo type");
Check(ammo.Count == 50 && ammo.Casing == "9mm_casing", "Ammo fields");
Check(ammo.AdditionalProperties?["damage"].ValueKind == JsonValueKind.Object,
    "Unknown damage structure retained");

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
