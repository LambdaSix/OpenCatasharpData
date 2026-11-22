using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Json;
using OpenCatasharpData.Resolution;

namespace OpenCatasharpData.Benchmarks;

/// <summary>
/// Resolves in-memory source records; fixture preparation and parsing into
/// CddaSourceRecord happen once in GlobalSetup, outside timed operations.
/// </summary>
[MemoryDiagnoser]
[JsonExporterAttribute.Full]
public sealed class ResolutionBenchmarks
{
    private CddaSourceRecord[] _records = null!;
    private CddaItemResolver _resolver = null!;

    private const string Parent = """
        {
          "type":"ITEM","abstract":"bench_base","subtypes":["AMMO"],
          "weight":"12 g","volume":"194 ml","recoil":1500,
          "damage":{"damage_type":"bullet","amount":39,"armor_penetration":2},
          "flags":["IRREPLACEABLE_CONSUMABLE"]
        }
        """;

    private const string Child = """
        {
          "type":"ITEM","id":"reloaded_223",
          "copy-from":"bench_base","subtypes":["COMESTIBLE"],
          "calories":120,"relative":{"damage":{"damage_type":"bullet","amount":-3}},
          "proportional":{"recoil":1.1,"weight":0.95},
          "extend":{"flags":["RECYCLED"]},
          "delete":{"flags":["IRREPLACEABLE_CONSUMABLE"]}
        }
        """;

    [GlobalSetup]
    public void Setup()
    {
        _resolver = new CddaItemResolver();
        var records = new List<CddaSourceRecord>
        {
            CddaSourceLoader.ParseRecord(Parent)
        };

        for (var i = 0; i < 100; i++)
        {
            var source = Child.Replace("reloaded_223", $"reloaded_{i:D3}",
                StringComparison.Ordinal);
            records.Add(CddaSourceLoader.ParseRecord(source, "bench", i));
        }
        _records = records.ToArray();

        var check = _resolver.Resolve(_records);
        if (check.Items.Count != 100)
            throw new InvalidOperationException("Unexpected benchmark fixture count.");
    }

    [Benchmark]
    public CddaResolvedItemCatalog Resolve100PreparedItems() => _resolver.Resolve(_records);

    [Benchmark]
    public CddaSourceRecord ParseOneSourceRecord() =>
        CddaSourceLoader.ParseRecord(Child);
}
