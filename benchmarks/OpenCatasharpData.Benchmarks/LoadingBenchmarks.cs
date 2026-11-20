using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Json;
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

namespace OpenCatasharpData.Benchmarks;

/// <summary>
/// Batch and disk I/O operations. Setup creates files and catalog fixtures
/// once per benchmark process, never within measured parsing operations.
/// </summary>
[MemoryDiagnoser]
[JsonExporterAttribute.Full]
public class LoadingBenchmarks
{
    private string _directory = null!;
    private string _singleFile = null!;
    private IReadOnlyList<LoadedDefinition> _seedEntries = null!;
    private CddaDataCatalog _catalog = null!;

    [GlobalSetup]
    public void Setup()
    {
        _directory = Path.Combine(
            Path.GetTempPath(), "opencatasharp-bench-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        Directory.CreateDirectory(Path.Combine(_directory, "nested"));

        for (var i = 0; i < 12; i++)
        {
            var parent = i % 2 == 0 ? _directory : Path.Combine(_directory, "nested");
            var path = Path.Combine(parent, $"items-{i:D2}.json");
            File.WriteAllText(path, FixtureData.BatchJson);
            if (i == 0) _singleFile = path;
        }

        var entries = new List<LoadedDefinition>();
        for (var i = 0; i < 256; i++)
        {
            var source = FixtureData.Comestible.Replace(
                "\"apple_cider\"", $"\"food_{i:D4}\"", StringComparison.Ordinal);
            entries.Add(new LoadedDefinition(
                _singleFile, CddaRecordParser.Default.Parse(source)));
        }

        _seedEntries = entries;
        _catalog = new CddaDataCatalog(entries);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }

    [Benchmark]
    public async Task<int> LoadMemoryStream()
    {
        using var stream = new MemoryStream(FixtureData.BatchUtf8, writable: false);
        var definitions = await CddaJsonLoader.LoadAsync(stream);
        return definitions.Count;
    }

    [Benchmark]
    public Task<IReadOnlyList<CddaDefinition>> LoadSingleFile() =>
        CddaJsonLoader.LoadFileAsync(_singleFile);

    // End-to-end: recursive disk enumeration + JSON parsing + index construction.
    [Benchmark]
    public Task<CddaDataCatalog> LoadDirectoryCatalog() =>
        CddaJsonLoader.LoadDirectoryCatalogAsync(_directory);

    // In-memory catalogue construction isolated from disk / parsing overhead.
    [Benchmark]
    public CddaDataCatalog ConstructCatalog() => new(_seedEntries);

    [Benchmark]
    public IReadOnlyList<LoadedDefinition> LookupById() => _catalog.FindById("food_0128");

    [Benchmark]
    public IReadOnlyList<ComestibleItemDefinition> GetByType() =>
        _catalog.Get<ComestibleItemDefinition>();
}
