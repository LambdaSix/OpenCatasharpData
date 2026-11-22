using System.Text.Json;
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

internal static class CompositionSmoke
{
    public static void Run()
    {
        // This single source record must produce *three* independently
        // deserialised slot objects, irrespective of declaration order.
        var item = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {
              "type":"ITEM","id":"multi-slot",
              "subtypes":["GUN","ARMOR","COMESTIBLE"],
              "armor":[{"covers":["torso"],"coverage":75}],
              "dispersion":480,"calories":120,
              "stim":-12,"sleepiness_mod":3,"healthy":-2,
              "addiction_potential":80,
              "addiction_type":["alcohol",{"addiction":"cocaine","potential":5}],
              "monotony_penalty":2,"quench":10,"fun":20,
              "spoils_in":"6 days","comestible_type":"DRINK",
              "tool":"apparatus","stack_size":8,
              "seasonings":["seasonings_universal"],
              "freezing_point":-8,
              "cooks_like":"meat_cooked","eats_like":"butter",
              "parasites":10,
              "contamination":[{"disease":"bad_food","probability":0.1}],
              "rot_spawn":{"monster":"mon_moose_calf","amount":[1,3],"chance":70},
              "smoking_result":"dry_meat","petfood":["CATTLEFOOD"],
              "consumption_effect_on_conditions":[
                "EOC_1",
                {"id":"EOC_2","effect":{"u_message":"some message"}}
              ],
              "unknown_mod_field":true
            }
            """);
        Check(item.GunData?.Dispersion == 480 && item.ArmorData?.Sections?[0].Coverage == 75,
            "Gun and armour slots both present");
        var food = item.ComestibleData;
        Check(food is not null && food.Calories == 120 &&
            food.Stimulation == -12 && food.SleepinessModifier == 3 &&
            food.Health == -2 && food.AddictionPotential == 80,
            "Eager nutritional and physiological properties");
        Check(food.Addictions?.Count == 2 &&
            food.Addictions[0].Name == "alcohol" &&
            food.Addictions[0].WasString &&
            food.Addictions[1].Name == "cocaine" &&
            food.Addictions[1].Potential == 5,
            "String and object addictions become typed records");
        Check(food.ConsumptionEffects?.Count == 2 &&
            food.ConsumptionEffects[0].Id == "EOC_1" &&
            food.ConsumptionEffects[1].Id == "EOC_2" &&
            food.ConsumptionEffects[1].AdditionalProperties?["effect"].ValueKind == JsonValueKind.Object,
            "EOC references and inline objects eagerly classified");
        Check(food.MonotonyPenalty == 2 && food.Quench == 10 &&
            food.Fun == 20 && food.SpoilsIn?.Days == 6 &&
            food.ComestibleType == "DRINK", "Consumption metadata");
        Check(food.RequiredTool == "apparatus" && food.StackSize == 8 &&
            food.Seasonings?[0] == "seasonings_universal" &&
            food.FreezingPointCelsius == -8 &&
            food.CooksLike == "meat_cooked" && food.EatsLike == "butter",
            "Cooking, freezing and serving properties");
        Check(food.Parasites == 10 &&
            food.Contamination?[0].Disease == "bad_food" &&
            food.Contamination[0].Probability == 0.1m,
            "Contamination typed");
        Check(food.RotSpawn?.Monster == "mon_moose_calf" &&
            food.RotSpawn.Amount?.Minimum == 1 &&
            food.RotSpawn.Amount?.Maximum == 3 &&
            food.RotSpawn.Chance == 70, "Rot spawn and numeric ranges typed");
        Check(food.SmokingResult == "dry_meat" && food.PetFood?[0] == "CATTLEFOOD",
            "Smoking and pet food fields");
        Check(item.GetSubtype<ComestibleData>() == item.ComestibleData &&
            item.GetSubtype<GunData>() == item.GunData &&
            item.GetSubtype<ArmorData>() == item.ArmorData &&
            item.GetSubtype<ToolData>() is null,
            "Typed subtype lookup does not parse again");
        Check(item.AdditionalProperties?["unknown_mod_field"].GetBoolean() == true,
            "Unknown fields remain available");

        // Reordered slot lists and missing specialised fields still produce
        // the right slots; no first-match-wins behaviour.
        var reordered = (ItemDefinition)CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"reordered","subtypes":["TOOL","ARMOR","MAGAZINE"],
             "charges_per_use":2,"armor":[{"covers":["head"]}],
             "capacity":5,"ammo_type":["battery"]}
            """);
        Check(reordered.ToolData?.ChargesPerUse == 2 &&
            reordered.ArmorData?.Sections?[0].Covers?[0] == "head" &&
            reordered.MagazineData?.Capacity == 5 &&
            reordered.AmmoTypes?[0] == "battery", "All declared slots applied");

        // Existing top-level handler plugins remain supported.
        var handlers = CddaDefaultHandlers.Create();
        var custom = new CddaRecordParser(handlers);
        var result = custom.Parse("""{"type":"ITEM","id":"plain"}""");
        Check(result is ItemDefinition { ArmorData: null, ComestibleData: null },
            "Generic items need no slot");

        // Independent custom subtype parsers attach extra objects without
        // changing the record parser or directory loader.
        var slotRegistry = new List<ICddaItemSubtypeHandler>(
            CddaDefaultHandlers.CreateItemSubtypeHandlers())
        {
            new JsonItemSubtypeHandler<CustomFoodTag>("CUSTOM_FOOD")
        };
        var pluginParser = new CddaRecordParser(CddaDefaultHandlers.Create(slotRegistry));
        var customItem = (ItemDefinition)pluginParser.Parse(
            """{"type":"ITEM","id":"modded","subtypes":["CUSTOM_FOOD","COMESTIBLE"],"tag":"my_food","calories":50}""");
        Check(customItem.GetSubtype<CustomFoodTag>()?.Tag == "my_food" &&
            customItem.ComestibleData?.Calories == 50,
            "Custom and builtin subtype parsers coexist");

        Console.WriteLine("Composed item and comestible DTO smoke tests passed.");
    }

    private static void Check(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException("FAILED: " + label);
    }

    public sealed class CustomFoodTag
    {
        [System.Text.Json.Serialization.JsonPropertyName("tag")]
        public string? Tag { get; set; }
    }
}
