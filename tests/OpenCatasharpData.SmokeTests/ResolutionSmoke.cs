using System.Text.Json;
using OpenCatasharpData.Models;
using OpenCatasharpData.Resolution;

internal static class ResolutionSmoke
{
    private const string Parent = """
        {
          "type":"ITEM","abstract":"base_shell",
          "subtypes":["ARMOR","AMMO"],
          "name":{"str":"base shell"},
          "weight":"100 g","volume":"1 L","price":"100 USD",
          "flags":["PSEUDO","IRREPLACEABLE_CONSUMABLE"],
          "armor":[{"covers":["torso"],"coverage":85}],
          "damage":{"damage_type":"bullet","amount":39,"armor_penetration":2},
          "recoil":1500,"dispersion":30,
          "effects":["COOKOFF"],"melee_damage":{"bash":4}
        }
        """;

    private const string Child = """
        {
          "type":"ITEM","id":"enhanced_shell","copy-from":"base_shell",
          "subtypes":["COMESTIBLE"],
          "name":{"str":"enhanced shell"},
          "calories":50,"healthy":-1,
          "relative":{
            "damage":{"damage_type":"bullet","amount":-3,"armor_penetration":10},
            "dispersion":20,
            "melee_damage":{"bash":2}
          },
          "proportional":{"weight":1.5,"volume":0.5,"price":0.7,"recoil":1.1},
          "extend":{"flags":["INCENDIARY"],"effects":["RECYCLED"]},
          "delete":{"flags":["PSEUDO"]}
        }
        """;

    private const string Grandchild = """
        {
          "type":"ITEM","id":"reloaded_shell","copy-from":"enhanced_shell",
          "subtypes":["TOOL"],
          "charges_per_use":2,
          "relative":{"damage":{"damage_type":"bullet","amount":-1}},
          "extend":{"flags":["RELOADED"]},
          "delete":{"flags":["IRREPLACEABLE_CONSUMABLE"]}
        }
        """;

    public static async Task RunAsync()
    {
        var parent = CddaSourceLoader.ParseRecord(Parent, "base.json", 0);
        var child = CddaSourceLoader.ParseRecord(Child, "child.json", 0);
        var grandchild = CddaSourceLoader.ParseRecord(Grandchild, "nested/grand.json", 0);

        // The caller may supply definitions in any order; forward references
        // and multiple inheritance levels are resolved independently of order.
        var catalog = new CddaItemResolver().Resolve([grandchild, child, parent]);
        Check(catalog.SourceRecords.Count == 3 &&
            catalog.Items.Count == 2 && catalog.Templates.Count == 1,
            "Separate raw sources, concrete items and abstract templates");

        var resolved = catalog.FindById("enhanced_shell");
        Check(resolved is not null && resolved.Source.CopyFrom == "base_shell" &&
            resolved.Source.RawJson.TryGetProperty("relative", out _),
            "Provenance and untouched source directives");
        var item = resolved!.Definition;

        Check(item.IdString == "enhanced_shell" && item.Abstract is null &&
            item.CopyFrom is null && item.Relative is null &&
            item.Name?.Singular == "enhanced shell",
            "Effective identity and inherited metadata");
        Check(item.Subtypes?.SequenceEqual(["COMESTIBLE"]) == true &&
            resolved.EffectiveSubtypes.SequenceEqual(["ARMOR", "AMMO", "COMESTIBLE"]),
            "Own subtype declarations distinguish effective inherited slots");

        Check(item.ArmorData?.Sections?[0].Coverage == 85m &&
            item.AmmoData?.Damage?.DamageType == "bullet" &&
            item.AmmoData?.Damage?.Amount == 36m &&
            item.AmmoData?.Damage?.ArmorPenetration == 12m &&
            item.AmmoData?.Dispersion == 50 &&
            item.AmmoData?.Recoil == 1650m &&
            item.ComestibleData?.Calories == 50m &&
            item.ComestibleData?.Health == -1,
            "Deep numeric relative/proportional patches and all composed slots");

        Check(item.MeleeDamage?.Bash == 6m &&
            item.Volume?.Millilitres == 500m &&
            item.Weight?.Grams == 150m &&
            item.Price?.Text == "70 USD", "Typed measurements and monetary scalar updates");
        Check(item.Flags?.SequenceEqual(["IRREPLACEABLE_CONSUMABLE", "INCENDIARY"]) == true &&
            item.AmmoData?.Effects?.SequenceEqual(["COOKOFF", "RECYCLED"]) == true,
            "Array extend/delete retain inherited members");
        Check(!resolved.EffectiveJson.TryGetProperty("copy-from", out _) &&
            !resolved.EffectiveJson.TryGetProperty("relative", out _) &&
            !resolved.EffectiveJson.TryGetProperty("delete", out _),
            "Effective source contains no unapplied patch directives");

        var reload = catalog.FindById("reloaded_shell")!;
        Check(reload.Definition.AmmoData?.Damage?.Amount == 35m &&
            reload.Definition.ToolData?.ChargesPerUse == 2 &&
            reload.Definition.ArmorData is not null &&
            reload.Definition.ComestibleData is not null &&
            reload.EffectiveSubtypes.SequenceEqual(["ARMOR", "AMMO", "COMESTIBLE", "TOOL"]),
            "Chained inheritance and effective subtype composition");
        Check(reload.Definition.Flags?.SequenceEqual(["INCENDIARY", "RELOADED"]) == true,
            "Child modifications do not mutate the parent");
        Check(catalog.GetItemsWith<ArmorData>().Count == 2 &&
            catalog.GetItemsWith<ComestibleData>().Count == 2 &&
            catalog.GetItemsWith<ToolData>().Count == 1,
            "Resolved catalogue queries include inherited slots");

        // Resolve a custom item source without parsing other, unrelated types.
        var mixed = new CddaItemResolver().Resolve([
            CddaSourceLoader.ParseRecord(
                """{"type":"material","id":"steel","density":1.1}"""),
            CddaSourceLoader.ParseRecord("""{"type":"ITEM","id":"plain","name":"plain"}""")
        ]);
        Check(mixed.Items.Count == 1 && mixed.SourceRecords.Count == 2 &&
            mixed.Items[0].Definition.Name?.Singular == "plain",
            "Non-item source records remain independent and raw");

        ExpectFailure(
            [
                CddaSourceLoader.ParseRecord(
                    """{"type":"ITEM","id":"orphan","copy-from":"not_loaded"}""")
            ], "Missing copy-from parent");
        ExpectFailure(
            [
                CddaSourceLoader.ParseRecord(
                    """{"type":"ITEM","id":"first","copy-from":"second"}"""),
                CddaSourceLoader.ParseRecord(
                    """{"type":"ITEM","id":"second","copy-from":"first"}""")
            ], "Cyclic copy-from");
        ExpectFailure(
            [
                CddaSourceLoader.ParseRecord("""{"type":"ITEM","id":"duplicate"}"""),
                CddaSourceLoader.ParseRecord("""{"type":"ITEM","abstract":"duplicate"}""")
            ], "Duplicate item identifier");
        ExpectFailure(
            [
                CddaSourceLoader.ParseRecord(Parent),
                CddaSourceLoader.ParseRecord(
                    """{"type":"ITEM","id":"invalid","copy-from":"base_shell",
                         "relative":{"damage":{"damage_type":"stab","amount":1}}}""")
            ], "String discriminator");
        ExpectFailure(
            [
                CddaSourceLoader.ParseRecord(Parent),
                CddaSourceLoader.ParseRecord(
                    """{"type":"ITEM","id":"invalid","copy-from":"base_shell",
                         "proportional":{"weight":{"bad":2}}}""")
            ], "Numeric patch");

        var directory = Path.Combine(Path.GetTempPath(), "cdda-resolution-" + Guid.NewGuid().ToString("N"));
        var nested = Path.Combine(directory, "nested");
        Directory.CreateDirectory(nested);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(directory, "parent.json"), "[" + Parent + "]");
            await File.WriteAllTextAsync(Path.Combine(directory, "child.json"), "[" + Child + "]");
            await File.WriteAllTextAsync(Path.Combine(nested, "grand.json"), "[" + Grandchild + "]");

            var loaded = await CddaSourceLoader.LoadResolvedItemsAsync(directory);
            Check(loaded.Items.Count == 2 && loaded.Templates.Count == 1,
                "Opt-in resolved directory loader");
            Check(loaded.FindById("reloaded_shell")?.Definition.AmmoData?.Damage?.Amount == 35m,
                "Directory resolver follows cross-file inheritance");
            var raw = await CddaSourceLoader.LoadDirectoryRecordsAsync(directory);
            Check(raw.Count == 3 && raw.All(entry => entry.RawJson.ValueKind == JsonValueKind.Object),
                "Raw directory loading retains independently owned JSON");
        }
        finally
        {
            Directory.Delete(directory, true);
        }

        Console.WriteLine("Source/ITEM resolution smoke tests passed.");
    }

    public static async Task RunPinnedForkExamplesAsync(string itemDirectory)
    {
        var file = Path.Combine(itemDirectory, "ammo", "223.json");
        if (!File.Exists(file))
            throw new FileNotFoundException("Pinned-fork resolution fixture is missing.", file);

        var raw = await CddaSourceLoader.LoadFileAsync(file);
        var selected = raw.Where(entry => entry.Id is "223" or "556").ToArray();
        Check(selected.Length == 2, "Pinned fork provides 223 and derived 556 definitions");

        var resolved = new CddaItemResolver().Resolve(selected);
        var original = resolved.FindById("223")?.Definition;
        var inherited = resolved.FindById("556")?.Definition;
        Check(original?.AmmoData?.Damage?.Amount == 39m &&
            inherited?.AmmoData?.Damage?.Amount == 36m &&
            inherited?.AmmoData?.Damage?.ArmorPenetration == 12m,
            "Real CDDA 223 -> 556 nested relative damage");
        Check(inherited.AmmoData?.Recoil == original.AmmoData?.Recoil * 1.1m,
            "Real CDDA 223 -> 556 proportional recoil");
        Console.WriteLine("Pinned-fork 223/556 inheritance fixture passed.");
    }

    private static void ExpectFailure(
        IEnumerable<CddaSourceRecord> source, string expectedFragment)
    {
        try
        {
            new CddaItemResolver().Resolve(source);
        }
        catch (CddaResolutionException error) when (
            error.Message.Contains(expectedFragment, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        throw new InvalidOperationException($"FAILED: Expected resolution error containing '{expectedFragment}'.");
    }

    private static void Check(bool value, string description)
    {
        if (!value) throw new InvalidOperationException("FAILED: " + description);
    }
}
