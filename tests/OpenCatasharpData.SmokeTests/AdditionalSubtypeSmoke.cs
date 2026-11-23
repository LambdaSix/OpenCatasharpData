using System.Text.Json;
using OpenCatasharpData;
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

internal static class AdditionalSubtypeSmoke
{
    public static void Run()
    {
        var bionic = (ItemDefinition)CddaRecordParser.Default.Parse(
            """{"type":"ITEM","id":"bio_adrenaline","subtypes":["BIONIC_ITEM"],"difficulty":6,"is_upgrade":false}""");
        Check(bionic.BionicItemData?.Difficulty == 6 &&
            bionic.BionicItemData.IsUpgrade == false,
            "Bionic installation difficulty and upgrade flag");

        var seed = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"seed_blackberries","subtypes":["SEED","COMESTIBLE"],
             "plant_name":"blackberry","fruit":"blackberries","seeds":false,
             "byproducts":["withered"],"growth_temp":"7 C",
             "required_terrain_flag":"PLANTABLE",
             "growth_stages":[{"GROWTH_SEED":"5 days"},{"GROWTH_SEEDLING":"10 days"},
                              {"GROWTH_HARVEST":"2 days"}],
             "calories":1}
            """);
        Check(seed.SeedData?.PlantName == "blackberry" &&
            seed.SeedData.Fruit == "blackberries" &&
            seed.SeedData.ProducesSeeds == false &&
            seed.SeedData.Byproducts?.Single() == "withered" &&
            seed.SeedData.GrowthTemperature?.Text == "7 C" &&
            seed.SeedData.RequiredTerrainFlag == "PLANTABLE" &&
            seed.SeedData.GrowthStages?[1]["GROWTH_SEEDLING"].Days == 10m &&
            seed.ComestibleData?.Calories == 1m,
            "Seed growth stages, temperatures and combined edible subtype");

        var brew = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"brew_whiskey","subtypes":["BREWABLE","COMESTIBLE"],
             "brew_time":"7 days",
             "brew_results":["wash_whiskey",{"item":"yeast","variant":"active","count":3}],
             "calories":120}
            """);
        Check(brew.BrewableData?.BrewTime?.Days == 7m &&
            brew.BrewableData.BrewResults?.Results.Count == 2 &&
            brew.BrewableData.BrewResults.Results[0].Item == "wash_whiskey" &&
            brew.BrewableData.BrewResults.Results[0].WasString &&
            brew.BrewableData.BrewResults.Results[1].Variant == "active" &&
            brew.BrewableData.BrewResults.Results[1].Count?.Number == 3m &&
            brew.ComestibleData?.Calories == 120m,
            "Brewable time and mixed string/object output forms");
        using var arrayResult = JsonDocument.Parse(
            JsonSerializer.Serialize(brew.BrewableData.BrewResults));
        Check(arrayResult.RootElement.ValueKind == JsonValueKind.Array &&
            arrayResult.RootElement[0].ValueKind == JsonValueKind.String,
            "Brewable array output shape round-trips");

        var mapBrew = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"yeast_brew","subtypes":["BREWABLE"],
             "brew_results":{"wash_whiskey":2,"yeast":3}}
            """);
        Check(mapBrew.BrewableData?.BrewResults?.WasMap == true &&
            mapBrew.BrewableData.BrewResults.Results[0].Count?.Number == 2m &&
            mapBrew.BrewableData.BrewResults.Results[1].Item == "yeast",
            "Map-form brew results become typed yields");
        using var mapResult = JsonDocument.Parse(
            JsonSerializer.Serialize(mapBrew.BrewableData!.BrewResults));
        Check(mapResult.RootElement.ValueKind == JsonValueKind.Object &&
            mapResult.RootElement.GetProperty("yeast").GetInt32() == 3,
            "Map-form brewing output shape round-trips");

        var compost = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"fermentable_slurry","subtypes":["COMPOSTABLE","COMESTIBLE"],
             "compost_time":"60 days",
             "compost_results":{"fermented_fertilizer_liquid":1,"biogas":250},
             "calories":0}
            """);
        Check(compost.CompostableData?.CompostTime?.Days == 60m &&
            compost.CompostableData.CompostResults?.Results[1].Item == "biogas" &&
            compost.CompostableData.CompostResults.Results[1].Count?.Number == 250m &&
            compost.ComestibleData is not null,
            "Compostable output map and composite subtype");

        var milling = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"pistachio_shelled","subtypes":["MILLING","COMESTIBLE"],
             "into":"paste_nut","recipe":"paste_nut_mill_10_1","calories":20}
            """);
        Check(milling.MillingData?.ResultItem == "paste_nut" &&
            milling.MillingData.Recipe == "paste_nut_mill_10_1",
            "Milling transformation and recipe ID");

        var engine = (ItemDefinition)CddaRecordParser.Default.Parse(
            """{"type":"ITEM","id":"engine_diesel","subtypes":["ENGINE"],"flags":["ENGINE"]}""");
        Check(engine.EngineData is not null && engine.Flags?.Single() == "ENGINE",
            "Marker-only engine data remains an independent subtype");

        var wheel = (ItemDefinition)CddaRecordParser.Default.Parse(
            """{"type":"ITEM","id":"wheel","subtypes":["WHEEL"],"diameter":17,"width":9}""");
        Check(wheel.WheelData?.Diameter == 17m && wheel.WheelData.Width == 9m,
            "Vehicle wheel geometry is eager typed");

        var petArmor = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"horse_armor","subtypes":["PET_ARMOR"],
             "min_pet_vol":"380 L","max_pet_vol":"1000 L","pet_bodytype":"horse",
             "material_thickness":10,"environmental_protection":2}
            """);
        Check(petArmor.PetArmorData?.MinimumPetVolume?.Litres == 380m &&
            petArmor.PetArmorData.MaximumPetVolume?.Litres == 1000m &&
            petArmor.PetArmorData.PetBodyType == "horse" &&
            petArmor.PetArmorData.MaterialThickness == 10m,
            "Pet equipment dimensions and physical protection");

        var toolMod = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"battery_toolmod","subtypes":["TOOLMOD"],
             "acceptable_ammo":["battery"],"ammo_modifier":["battery"],
             "magazine_adaptor":[["battery",["small_storage_battery"]]],
             "pocket_mods":[{"pocket_type":"MAGAZINE_WELL",
                             "item_restriction":["small_storage_battery"]}],
             "consumption_mods":[{"pocket":"power","multiply":0.5}],
             "capacity_mods":[{"pocket":"power","multiply":2}]}
            """);
        Check(toolMod.ToolModData?.AcceptableAmmo?.Single() == "battery" &&
            toolMod.ToolModData.MagazineAdaptors?[0].AmmoType == "battery" &&
            toolMod.ToolModData.PocketMods?[0].ItemRestriction?[0] == "small_storage_battery" &&
            toolMod.ToolModData.ConsumptionMods?[0].Multiply == 0.5m &&
            toolMod.ToolModData.CapacityMods?[0].Multiply == 2m,
            "Toolmod compatibility, magazine adapters and pocket modifiers");

        var artifact = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"string_dimension_hypercube","subtypes":["ARTIFACT"],
             "passive_effects":[{"has":"HELD","condition":"ALWAYS",
                                 "values":[{"value":"ARTIFACT_RESONANCE","add":2000}],
                                 "unknown_enchantment":"retained"}],
             "effects_carried":["AEP_GLOW"]}
            """);
        Check(artifact.ArtifactData?.PassiveEffects?[0].Has == "HELD" &&
            artifact.ArtifactData.PassiveEffects[0].Condition == "ALWAYS" &&
            artifact.ArtifactData.PassiveEffects[0].Values?[0].Value == "ARTIFACT_RESONANCE" &&
            artifact.ArtifactData.PassiveEffects[0].Values?[0].Add == 2000m &&
            artifact.ArtifactData.PassiveEffects[0].AdditionalProperties?["unknown_enchantment"].GetString() == "retained" &&
            artifact.ArtifactData.EffectsCarried?[0] == "AEP_GLOW",
            "Artifact passive enchantments are eager typed and extensible");

        Console.WriteLine("Remaining ITEM subtype smoke tests passed.");
    }

    public static async Task RunPinnedForkAsync(string itemDirectory)
    {
        var checks = new (string File, string Subtype)[]
        {
            ("bionics.json", "BIONIC_ITEM"),
            ("comestibles/seed.json", "SEED"),
            ("comestibles/brewing.json", "BREWABLE"),
            ("comestibles/nuts.json", "MILLING"),
            ("vehicle/engine.json", "ENGINE"),
            ("vehicle/wheel.json", "WHEEL"),
            ("armor/pets_cow_armor.json", "PET_ARMOR"),
            ("toolmod.json", "TOOLMOD"),
            ("relics/extradimensional_artifacts.json", "ARTIFACT")
        };

        foreach (var (file, subtype) in checks)
        {
            var records = await CddaJsonLoader.LoadFileAsync(
                Path.Combine(itemDirectory, file.Replace('/', Path.DirectorySeparatorChar)));
            var item = records.OfType<ItemDefinition>().FirstOrDefault(record =>
                record.Subtypes?.Contains(subtype) == true);
            Check(item is not null, $"Pinned fork contains a {subtype} record ({file})");
            var populated = subtype switch
            {
                "BIONIC_ITEM" => item!.BionicItemData is not null,
                "SEED" => item!.SeedData?.PlantName is not null,
                "BREWABLE" => item!.BrewableData?.BrewTime is not null,
                "MILLING" => item!.MillingData?.ResultItem is not null,
                "ENGINE" => item!.EngineData is not null,
                "WHEEL" => item!.WheelData is not null,
                "PET_ARMOR" => item!.PetArmorData is not null,
                "TOOLMOD" => item!.ToolModData is not null,
                "ARTIFACT" => item!.ArtifactData?.PassiveEffects is not null,
                _ => false
            };
            Check(populated, $"Pinned {subtype} component eager loaded");
        }
        Console.WriteLine("Pinned CDDA additional ITEM subtype fixtures passed.");
    }

    private static void Check(bool success, string label)
    {
        if (!success) throw new InvalidOperationException("FAILED: " + label);
    }
}
