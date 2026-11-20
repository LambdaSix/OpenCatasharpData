using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Json;
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

namespace OpenCatasharpData.Benchmarks;

/// <summary>Pure record parsing; JSON source strings are prepared before measurement.</summary>
[MemoryDiagnoser]
[JsonExporterAttribute.Full]
public class RecordParsingBenchmarks
{
    private readonly CddaRecordParser _parser = CddaRecordParser.Default;
    private JsonDocument _document = null!;

    [GlobalSetup]
    public void Setup() => _document = JsonDocument.Parse(FixtureData.Armor);

    [GlobalCleanup]
    public void Cleanup() => _document.Dispose();

    [Benchmark]
    public CddaDefinition ParseGeneric() => _parser.Parse(FixtureData.Generic);

    [Benchmark]
    public CddaDefinition ParseComestible() => _parser.Parse(FixtureData.Comestible);

    [Benchmark]
    public CddaDefinition ParseArmor() => _parser.Parse(FixtureData.Armor);

    [Benchmark]
    public CddaDefinition ParseGun() => _parser.Parse(FixtureData.Gun);

    [Benchmark]
    public CddaDefinition ParseMagazine() => _parser.Parse(FixtureData.Magazine);

    // Separately measures handler dispatch / DTO deserialization without
    // re-parsing the JSON text into a DOM each time.
    [Benchmark]
    public CddaDefinition ParsePreparsedJsonElement() => _parser.Parse(_document.RootElement);
}
