using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Json;
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

namespace OpenCatasharpData.Benchmarks;

/// <summary>Independent workloads for energy measurement and new ITEM subtype DTOs.</summary>
[MemoryDiagnoser]
[JsonExporterAttribute.Full]
public class AdditionalItemParsingBenchmarks
{
    private readonly CddaRecordParser _parser = CddaRecordParser.Default;

    private const string Seed = """
        {
          "type":"ITEM","id":"seed_blackberries","subtypes":["SEED","COMESTIBLE"],
          "plant_name":"blackberry","fruit":"blackberries","seeds":false,
          "byproducts":["withered"],
          "growth_temp":"7 C","growth_stages":[
            {"GROWTH_SEED":"5 days"},{"GROWTH_SEEDLING":"10 days"},
            {"GROWTH_HARVEST":"2 days"}],
          "calories":50
        }
        """;

    private const string Brewable = """
        {
          "type":"ITEM","id":"brew_whiskey","subtypes":["BREWABLE","COMESTIBLE"],
          "brew_time":"7 days",
          "brew_results":["wash_whiskey",{"item":"yeast","count":3}],
          "calories":120
        }
        """;

    private const string Bionic = """
        {
          "type":"ITEM","id":"bio_adrenaline","subtypes":["BIONIC_ITEM"],
          "difficulty":6,"is_upgrade":false
        }
        """;

    private const string Artifact = """
        {
          "type":"ITEM","id":"dimension_hypercube","subtypes":["ARTIFACT"],
          "passive_effects":[
            {"has":"HELD","condition":"ALWAYS",
             "values":[{"value":"ARTIFACT_RESONANCE","add":2000}]}
          ]
        }
        """;

    private const string Toolmod = """
        {
          "type":"ITEM","id":"mod_battery","subtypes":["TOOLMOD"],
          "acceptable_ammo":["battery"],
          "magazine_adaptor":[["battery",["small_storage_battery"]]],
          "pocket_mods":[
            {"pocket_type":"MAGAZINE_WELL",
             "item_restriction":["small_storage_battery"]}
          ]
        }
        """;

    private const string PoweredGun = """
        {
          "type":"ITEM","id":"powered_gun","subtypes":["GUN"],
          "energy_drain":"2 kJ","power_draw":"100 W",
          "heat_per_shot":10,"cooling_value":2
        }
        """;

    [Benchmark] public CddaDefinition ParseSeed() => _parser.Parse(Seed);
    [Benchmark] public CddaDefinition ParseBrewable() => _parser.Parse(Brewable);
    [Benchmark] public CddaDefinition ParseBionic() => _parser.Parse(Bionic);
    [Benchmark] public CddaDefinition ParseArtifact() => _parser.Parse(Artifact);
    [Benchmark] public CddaDefinition ParseToolmod() => _parser.Parse(Toolmod);
    [Benchmark] public CddaDefinition ParsePoweredGun() => _parser.Parse(PoweredGun);
}
