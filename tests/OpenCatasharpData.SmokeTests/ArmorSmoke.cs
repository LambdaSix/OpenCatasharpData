using System.Text.Json;
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

internal static class ArmorSmoke
{
    public static void Run()
    {
        var item = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {
              "type":"ITEM","id":"test_protective_vest",
              "subtypes":["ARMOR","TOOL"],
              "warmth":12,"material_thickness":2.5,
              "sided":true,"power_armor":false,"max_worn":1,
              "valid_mods":["steel_padded"],
              "non_functional":"damaged_vest",
              "damage_verb":"crunches",
              "environmental_protection":4,
              "encumbrance":[5,12],"max_encumbrance":15,
              "covers":["torso"],"coverage":90,"cover_melee":75,
              "cover_ranged":55,"cover_vitals":40,
              "charges_per_use":2,
              "armor":[
                {
                  "covers":["torso"],
                  "specifically_covers":["torso_upper"],
                  "material":[{"type":"kevlar","thickness":1.5}],
                  "encumbrance":[5,9],"coverage":90,
                  "cover_melee":80,"cover_ranged":60,"cover_vitals":30,
                  "layers":["OUTER"],"breathability":"GOOD",
                  "rigid_layer_only":true,
                  "volume_encumber_modifier":0.25,
                  "activity_noise":{"volume":8,"chance":60},
                  "custom_armor_tag":"retained"
                }
              ],
              "pocket_data":[
                {
                  "pocket_type":"CONTAINER",
                  "max_contains_volume":"2 L",
                  "max_contains_weight":"5 kg",
                  "max_item_length":"60 cm",
                  "min_item_length":"5 cm",
                  "min_item_volume":"150 ml",
                  "max_item_volume":"500 ml",
                  "default_magazine":"heavy_battery_cell",
                  "open_container":true,"airtight":true,
                  "extra_encumbrance":3,
                  "ripoff":2,"holster":true,
                  "weight_multiplier":0.8,"volume_multiplier":1.2,
                  "spoil_multiplier":0.9,
                  "inherits_flags":false,"fire_protection":true,
                  "name":"inner pocket",
                  "sealed_data":{"spoil_multiplier":0.5,"custom_seal":true},
                  "activity_noise":{"volume":3,"chance":10},
                  "custom_pocket_tag":42
                }
              ]
            }
            """);

        var armor = item.ArmorData;
        Check(armor is not null && item.ToolData?.ChargesPerUse == 2,
            "Armour and tool slots coexist");
        Check(armor.Warmth == 12 &&
            armor.MaterialThickness?.Number == 2.5m &&
            armor.Sided == true && armor.PowerArmor == false &&
            armor.MaximumWorn == 1 &&
            armor.ValidMods?.SequenceEqual(["steel_padded"]) == true,
            "Typed armour insulation, fittings and equipment mods");
        Check(armor.NonFunctionalItem == "damaged_vest" &&
            armor.DamageVerb == "crunches" &&
            armor.EnvironmentalProtection == 4m &&
            item.EnvironmentalProtection == 4m,
            "Damage transformations and shared environmental protection");
        Check(armor.Encumbrance?.Minimum == 5m &&
            armor.Encumbrance?.Maximum == 12m &&
            armor.MaximumEncumbrance == 15m,
            "Top-level armour encumbrance range");
        Check(armor.Covers?.SequenceEqual(["torso"]) == true &&
            armor.Coverage == 90m && armor.MeleeCoverage == 75m &&
            armor.RangedCoverage == 55m && armor.VitalCoverage == 40m,
            "Distinct top-level coverage categories");

        Check(ReferenceEquals(armor.Sections, item.Armor),
            "Armour sections reuse the eagerly parsed common list");
        var section = armor.Sections?.Single();
        Check(section is not null &&
            section.MeleeCoverage == 80m && section.RangedCoverage == 60m &&
            section.CoverVitals == 30m &&
            section.Breathability == "GOOD" &&
            section.RigidLayerOnly == true &&
            section.VolumeEncumbranceModifier == 0.25m,
            "Section coverage, breathable layers and rigidity");
        Check(section.ActivityNoise?.Volume == 8m &&
            section.ActivityNoise.Chance == 60m &&
            section.AdditionalProperties?["custom_armor_tag"].GetString() == "retained",
            "Typed section activity noise and unknown field preservation");

        var pocket = item.PocketData?.Single();
        Check(pocket is not null &&
            pocket.DefaultMagazine == "heavy_battery_cell" &&
            pocket.OpenContainer == true && pocket.Airtight == true &&
            pocket.FireProtection == true && pocket.InheritsFlags == false,
            "Container flags and magazine preferences");
        Check(pocket.MaxContainsVolume?.Millilitres == 2000m &&
            pocket.MaxContainsWeight?.Grams == 5000m &&
            pocket.MaxItemLength?.Centimetres == 60m &&
            pocket.MinimumItemLength?.Centimetres == 5m &&
            pocket.MinimumItemVolume?.Millilitres == 150m &&
            pocket.MaximumItemVolume?.Millilitres == 500m,
            "Dimensional constraints use dedicated value types");
        Check(pocket.ExtraEncumbrance == 3m && pocket.Ripoff == 2m &&
            pocket.WeightMultiplier == 0.8m && pocket.VolumeMultiplier == 1.2m &&
            pocket.SpoilMultiplier == 0.9m,
            "Pocket encumbrance and multipliers");
        Check(pocket.Name?.Singular == "inner pocket" &&
            pocket.SealedData?.SpoilMultiplier == 0.5m &&
            pocket.SealedData.AdditionalProperties?["custom_seal"].GetBoolean() == true &&
            pocket.ActivityNoise?.Volume == 3m &&
            pocket.AdditionalProperties?["custom_pocket_tag"].GetInt32() == 42,
            "Nested pocket metadata and extension fields retained");

        // Existing ARMOR properties can be omitted; absence is not a hardcoded
        // game default, which is important for copy-from and patch resolution.
        var empty = (ItemDefinition)CddaRecordParser.Default.Parse(
            """{"type":"ITEM","id":"empty_armor","subtypes":["ARMOR"]}""");
        Check(empty.ArmorData is
            { Warmth: null, Sided: null, Coverage: null, Sections: null },
            "Omitted armour fields remain absent, not inferred defaults");

        // A full, typed section can also appear on an item with another subtype.
        var bookArmor = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"book_armor","subtypes":["BOOK","ARMOR"],
             "chapters":5,"armor":[{"covers":"head","cover_ranged":10}]}
            """);
        Check(bookArmor.BookData?.Chapters == 5 &&
            bookArmor.ArmorData?.Sections?[0].Covers?[0] == "head" &&
            bookArmor.ArmorData.Sections[0].RangedCoverage == 10m,
            "Composite BOOK/ARMOR item shares typed sections");

        Console.WriteLine("Expanded ARMOR and pocket DTO smoke tests passed.");
    }

    private static void Check(bool success, string description)
    {
        if (!success) throw new InvalidOperationException("FAILED: " + description);
    }
}
