using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Json;
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

namespace OpenCatasharpData.Benchmarks;

/// <summary>
/// Independent parse workloads for absolute temperatures, inline snippets
/// and mixed artifact effect forms. Existing benchmark baselines are unchanged.
/// </summary>
[MemoryDiagnoser]
[JsonExporterAttribute.Full]
public class TemperatureAndItemSchemaBenchmarks
{
    private readonly CddaRecordParser _parser = CddaRecordParser.Default;

    private const string TemperatureItem = """
        {
          "type":"ITEM","id":"test_seed","subtypes":["SEED","COMESTIBLE"],
          "plant_name":"blackberry","fruit":"blackberries",
          "growth_temp":"-23 C","freezing_point":-8,
          "growth_stages":[{"GROWTH_SEED":"5 days"}]
        }
        """;

    private const string ArtifactItem = """
        {
          "type":"ITEM","id":"artifact","subtypes":["ARTIFACT"],
          "passive_effects":[
            {"id":"ench_meteor_armor"},
            {"has":"WORN","condition":"ALWAYS","mutations":["well_distributed"],
             "values":[{"value":"ARTIFACT_RESONANCE","add":2000}]}
          ]
        }
        """;

    private const string MixedItem = """
        {
          "type":"ITEM","id":"mixed_fields",
          "snippet_category":[{"id":"patched","text":"Patched clothing."}],
          "countdown_interval":"24 hours",
          "countdown_action":{"type":"message","message":"The timer rang."},
          "charged_qualities":[["OVEN",2],{"id":"TEMPER","level":1}],
          "memory_card":{"data_chance":1,"recipes_amount":3},
          "transform_into":{"target":"pickled_egg","target_charges":10},
          "ememory_size":"128 MB","etransfer_rate":"30 MB",
          "proficiencies":[{"proficiency":"prof_intro_chemistry","time_factor":0.6}]
        }
        """;

    [Benchmark]
    public CddaTemperature ParseTemperature() =>
        JsonSerializer.Deserialize<CddaTemperature>("\"-23 C\"");

    [Benchmark]
    public CddaDefinition ParseSeedTemperature() => _parser.Parse(TemperatureItem);

    [Benchmark]
    public CddaDefinition ParseArtifactEffects() => _parser.Parse(ArtifactItem);

    [Benchmark]
    public CddaDefinition ParseCompleteItemMetadata() => _parser.Parse(MixedItem);
}
