using System.Runtime.CompilerServices;
using System.Text.Json;
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

namespace OpenCatasharpData;

/// <summary>
/// File and directory ingestion over an interchangeable record parser.
/// Deserializes raw CDDA source definitions; does not resolve copy-from.
/// </summary>
public static class CddaJsonLoader
{
    /// <summary>Load the array of definitions from one JSON file.</summary>
    public static async Task<IReadOnlyList<CddaDefinition>> LoadFileAsync(
        string filePath,
        CancellationToken cancellationToken = default,
        CddaRecordParser? parser = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        await using var stream = File.OpenRead(filePath);
        return await LoadAsync(stream, cancellationToken, parser).ConfigureAwait(false);
    }

    /// <summary>Load the array of definitions from a JSON stream.</summary>
    public static async Task<IReadOnlyList<CddaDefinition>> LoadAsync(
        Stream stream,
        CancellationToken cancellationToken = default,
        CddaRecordParser? parser = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        parser ??= CddaRecordParser.Default;

        using var document = await JsonDocument.ParseAsync(
            stream, CddaRecordParser.DocumentOptions, cancellationToken).ConfigureAwait(false);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new JsonException("Expected a top-level array of CDDA definitions.");

        var definitions = new List<CddaDefinition>();
        var index = 0;
        foreach (var record in document.RootElement.EnumerateArray())
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                definitions.Add(parser.Parse(record));
            }
            catch (JsonException exception)
            {
                throw new JsonException($"Failed to parse CDDA definition at array index {index}: {exception.Message}", exception);
            }
            index++;
        }

        return definitions;
    }

    /// <summary>Recursively stream definitions, processing one file at a time.</summary>
    public static async IAsyncEnumerable<LoadedDefinition> LoadDirectoryAsync(
        string directoryPath,
        [EnumeratorCancellation] CancellationToken cancellationToken = default,
        CddaRecordParser? parser = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        var root = Path.GetFullPath(directoryPath);
        if (!Directory.Exists(root)) throw new DirectoryNotFoundException(root);

        foreach (var file in Directory.EnumerateFiles(root, "*.json", SearchOption.AllDirectories)
                     .OrderBy(path => path, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var definitions = await LoadFileAsync(file, cancellationToken, parser).ConfigureAwait(false);

            foreach (var definition in definitions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return new LoadedDefinition(file, definition);
            }
        }
    }

    /// <summary>Recursively load and index all definitions in a directory.</summary>
    public static async Task<CddaDataCatalog> LoadDirectoryCatalogAsync(
        string directoryPath,
        CancellationToken cancellationToken = default,
        CddaRecordParser? parser = null)
    {
        var entries = new List<LoadedDefinition>();
        await foreach (var entry in LoadDirectoryAsync(directoryPath, cancellationToken, parser))
            entries.Add(entry);

        return new CddaDataCatalog(entries);
    }
}

/// <summary>Source filename alongside its already deserialized definition.</summary>
public sealed record LoadedDefinition(string FilePath, CddaDefinition Definition);
