using System.Text.Json;
using OpenCatasharpData;
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

internal static class SupplementaryFieldsSmoke
{
    public static void Run()
    {
        var item = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {
              "type":"ITEM","id":"complete_misc_fields",
              "snippet_category":[
                {"id":"patched","text":"A patched garment.","style":"custom"},
                "legacy_snippet"
              ],
              "container":"bag_plastic",
              "proficiencies":[
                {"proficiency":"prof_intro_chemistry","time_factor":0.6,
                 "fail_factor":0.25,"required":true}
              ],
              "countdown_interval":"24 hours",
              "repairs_with":["rag","leather_patch"],
              "light":8,
              "charged_qualities":[["OVEN",2],{"id":"TEMPER","level":1}],
              "countdown_action":{"type":"message","message":"A timer rang."},
              "martial_art":"aikido",
              "ememory_size":"128 MB",
              "etransfer_rate":"30 MB",
              "stackable":true,
              "environmental_protection_with_filter":7,
              "sub":"char_kiln",
              "insulation":10,
              "repairs_like":"jacket",
              "sight_dispersion":60,
              "emits":["emit_small_smoke_plume"],
              "expand_snippets":true,
              "variables":{"browsed":"true","water_per_tablet":4},
              "generic":true,
              "e_port":"phone",
              "consumption_per_use":[{"pocket":"battery","qty":1}],
              "properties":{"creature_size_capacity":"SMALL"},
              "source_monster":"mon_child",
              "transform_into":{"target":"pickled_egg",
                                 "container":"jar_glass_sealed","target_charges":10},
              "revert_msg":"It fades.",
              "drop":"gasbomb_act",
              "charge_factor":25,
              "nanofab_template_group":"constructs",
              "memory_card":{"data_chance":1,"recipes_chance":1,
                             "recipes_amount":5,"recipes_categories":["CC_AMMO"],
                             "recipes_level_max":5,"vendor_rule":"preserved"},
              "e_ports_banned":["USB-A"],
              "max_energy_shield_hp":100,
              "template_requirements":"nanofab_template",
              "fall_damage_reduction":20,
              "weight_multiplier":0.95,
              "min_str_required_mod_if_prone":-83,
              "fuel_efficiency":0.3
            }
            """);

        Check(item.SnippetCategory?.Snippets?.Count == 2 &&
            item.SnippetCategory.Snippets[0].Id == "patched" &&
            item.SnippetCategory.Snippets[0].Text?.Singular == "A patched garment." &&
            item.SnippetCategory.Snippets[0].AdditionalProperties?["style"].GetString() == "custom" &&
            item.SnippetCategory.Snippets[1].WasString,
            "Inline snippet objects and string snippet references are eager typed");
        using (var roundTrip = JsonDocument.Parse(JsonSerializer.Serialize(item)))
        {
            var field = roundTrip.RootElement.GetProperty("snippet_category");
            Check(field.ValueKind == JsonValueKind.Array &&
                field[0].GetProperty("text").GetString() == "A patched garment." &&
                field[1].GetString() == "legacy_snippet",
                "Snippet list source shape round-trips");
            Check(roundTrip.RootElement.GetProperty("countdown_action")
                .ValueKind == JsonValueKind.Object,
                "Countdown object remains an object during round-trip");
        }
        Check(item.Container == "bag_plastic" &&
            item.Proficiencies?[0].Proficiency == "prof_intro_chemistry" &&
            item.Proficiencies[0].TimeFactor == 0.6m &&
            item.Proficiencies[0].FailFactor == 0.25m &&
            item.Proficiencies[0].Required == true &&
            item.CountdownInterval?.Hours == 24m,
            "Container, proficiency and timer fields are typed");
        Check(item.RepairsWith?.SequenceEqual(["rag","leather_patch"]) == true &&
            item.Light == 8m && item.ChargedQualities?.Count == 2 &&
            item.ChargedQualities[0].Id == "OVEN" &&
            item.ChargedQualities[1].Level == 1m,
            "Repair materials, lighting and mixed charged quality formats");
        Check(item.CountdownAction?.Action?.Type == "message" &&
            item.CountdownAction.Action.AdditionalProperties?["message"].GetString() == "A timer rang." &&
            item.MartialArt == "aikido" &&
            item.ElectronicMemorySize?.Text == "128 MB" &&
            item.ElectronicTransferRate?.Text == "30 MB",
            "Countdown action, martial arts and electronic data sizes");
        Check(item.Stackable == true &&
            item.FilterEnvironmentalProtection == 7m &&
            item.Subcategory == "char_kiln" &&
            item.Insulation == 10m &&
            item.RepairsLike == "jacket" &&
            item.SightDispersion == 60m,
            "Equipment, insulation and source subtype metadata");
        Check(item.Emits?.Single() == "emit_small_smoke_plume" &&
            item.ExpandSnippets == true &&
            item.Variables?["water_per_tablet"].GetInt32() == 4 &&
            item.Generic == true &&
            item.ElectronicPort == "phone" &&
            item.ConsumptionPerUse?[0].Quantity == 1 &&
            item.Properties?["creature_size_capacity"].GetString() == "SMALL",
            "Dynamic source dictionaries and electronic/consumption metadata");
        Check(item.SourceMonster == "mon_child" &&
            item.TransformInto?.Target == "pickled_egg" &&
            item.TransformInto.TargetCharges?.Number == 10m &&
            item.RevertMessage?.Singular == "It fades." &&
            item.DropItem == "gasbomb_act" &&
            item.ChargeFactor == 25m &&
            item.NanofabricatorTemplateGroup == "constructs",
            "Source monsters, transformations and item lifecycle settings");
        Check(item.MemoryCard?.DataChance == 1m &&
            item.MemoryCard.RecipeCategories?.Single() == "CC_AMMO" &&
            item.MemoryCard.MaxRecipeLevel == 5 &&
            item.MemoryCard.AdditionalProperties?["vendor_rule"].GetString() == "preserved" &&
            item.BannedElectronicPorts?.Single() == "USB-A" &&
            item.MaximumEnergyShieldHitPoints == 100m &&
            item.TemplateRequirements == "nanofab_template" &&
            item.FallDamageReduction == 20m &&
            item.WeightMultiplier == 0.95m &&
            item.MinimumStrengthModifierWhenProne == -83m &&
            item.FuelEfficiency == 0.3m,
            "Memory-card metadata and remaining uncommon item fields");

        var snippetId = (ItemDefinition)CddaRecordParser.Default.Parse(
            """{"type":"ITEM","id":"snippet_id","snippet_category":"JACKET"}""");
        Check(snippetId.SnippetCategory?.CategoryId == "JACKET" &&
            snippetId.SnippetCategory.WasString &&
            JsonDocument.Parse(JsonSerializer.Serialize(snippetId)).RootElement
                .GetProperty("snippet_category").GetString() == "JACKET",
            "String category ID remains a string");

        var simpleCountdown = (ItemDefinition)CddaRecordParser.Default.Parse(
            """{"type":"ITEM","id":"simple_timer","countdown_action":"TRANSFORM","countdown_interval":"30 s"}""");
        Check(simpleCountdown.CountdownAction?.WasString == true &&
            simpleCountdown.CountdownAction.Action?.Type == "TRANSFORM" &&
            simpleCountdown.CountdownInterval?.Seconds == 30m &&
            JsonDocument.Parse(JsonSerializer.Serialize(simpleCountdown)).RootElement
                .GetProperty("countdown_action").GetString() == "TRANSFORM",
            "String-form countdown action round-trips");

        var enchantments = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"multi_artifact","subtypes":["ARTIFACT"],
             "passive_effects":[
               {"id":"ench_meteor_armor"},
               {"has":"WORN","condition":"ALWAYS",
                "mutations":["well_distributed"],
                "values":[{"value":"ARTIFACT_RESONANCE","add":2000}]}
             ],
             "effects_carried":["AEP_GLOW"],"effects_worn":["AEP_STEALTH"],
             "effects_wielded":["AEP_RESIST_ELECTRICITY"],
             "effects_activated":["AEA_STORM"]}
            """);
        var passive = enchantments.ArtifactData?.PassiveEffects;
        Check(passive?.Count == 2 &&
            passive[0].IsReference && passive[0].Id == "ench_meteor_armor" &&
            !passive[1].IsReference && passive[1].Has == "WORN" &&
            passive[1].Condition == "ALWAYS" &&
            passive[1].Mutations?.Single() == "well_distributed" &&
            passive[1].Values?[0].Value == "ARTIFACT_RESONANCE" &&
            passive[1].Values[0].Add == 2000m &&
            enchantments.ArtifactData?.EffectsCarried?.Single() == "AEP_GLOW" &&
            enchantments.ArtifactData?.EffectsWorn?.Single() == "AEP_STEALTH" &&
            enchantments.ArtifactData?.EffectsWielded?.Single() == "AEP_RESIST_ELECTRICITY" &&
            enchantments.ArtifactData?.EffectsActivated?.Single() == "AEA_STORM",
            "Artifact ID references, inline mutation effects and effect categories");

        Console.WriteLine("Complete top-level item and artifact-effect tests passed.");
    }

    public static async Task RunPinnedForkAsync(string root)
    {
        static async Task<ItemDefinition> Find(string path, string id)
        {
            var records = await CddaJsonLoader.LoadFileAsync(path);
            return records.OfType<ItemDefinition>().First(x => x.IdString == id);
        }

        var chemistry = await Find(Path.Combine(root, "book/chemistry.json"), "adv_chemistry");
        Check(chemistry.Proficiencies?[0].TimeFactor is not null,
            "Real book proficiency records");

        var coat = await Find(Path.Combine(root, "armor/coats.json"), "jacket_jean");
        Check(coat.SnippetCategory?.Snippets?.Count > 0 &&
            coat.SnippetCategory.Snippets[0].Id is not null,
            "Real inline armour snippet descriptions");

        var oven = await Find(Path.Combine(root, "fake.json"), "oven_tool");
        Check(oven.ChargedQualities?.Count > 0, "Real charged tool qualities");

        var timer = await Find(Path.Combine(root, "armor/exotic.json"), "migo_plate");
        Check(timer.CountdownInterval?.Hours == 24m &&
            timer.CountdownAction?.Action is not null,
            "Real item countdown action and duration");

        var efile = await Find(Path.Combine(root, "software.json"), "efile_recipes");
        Check(efile.MemoryCard?.RecipeCategories?.Count > 0, "Real memory card metadata");

        var shell = await Find(Path.Combine(root, "tool/cooking.json"), "eggs_ferment_active");
        Check(shell.TransformInto?.Target == "pickled_egg",
            "Real fermentation item transformation");

        var meteor = await Find(Path.Combine(root, "armor/exotic.json"), "armor_star_samurai");
        Check(meteor.ArtifactData?.PassiveEffects?[0].Id == "ench_meteor_armor" &&
            meteor.ArtifactData.PassiveEffects[0].IsReference,
            "Real artifact ID-based enchantment references");

        var nomad = await Find(
            Path.Combine(root, "armor/bespoke_armor/custom_bodysuits.json"), "armor_nomad");
        Check(nomad.ArtifactData?.PassiveEffects?[0].Mutations?.Contains("well_distributed") == true,
            "Real inline artifact mutations");

        Console.WriteLine("Pinned fork uncommon item and artifact fixtures passed.");
    }

    private static void Check(bool ok, string label)
    {
        if (!ok) throw new InvalidOperationException("FAILED: " + label);
    }
}
