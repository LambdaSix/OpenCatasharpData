using System.Runtime.CompilerServices;
using System.Text.Json;
using OpenCatasharpData.Parsing;

namespace OpenCatasharpData.Resolution;

/// <summary>
/// One untouched JSON source definition. RawJson owns a cloned JSON value and
/// remains usable after the source file's JsonDocument has been disposed.
/// It is deliberately separate from eagerly deserialized DTOs.
/// </summary>
public sealed record CddaSourceRecord(string FilePath, int RecordIndex, JsonElement RawJson)
{
    public string? Type => GetString("type");
    public string? Id => GetString("id");
    public string? AbstractId => GetString("abstract");
    public string? CopyFrom => GetString("copy-from");

    private string? GetString(string name) =>
        RawJson.ValueKind == JsonValueKind.Object &&
        RawJson.TryGetProperty(name, out var value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    public override string ToString() =>
        $"{FilePath}[{RecordIndex}] ({Id ?? AbstractId ?? Type ?? "unknown"})";
}

/// <summary>
/// Reads source JSON without materialising a DTO or applying inheritance.
/// Source order follows CDDA's breadth-first directory traversal with
/// lexically ordered sibling files/directories.
/// </summary>
public static class CddaSourceLoader
{
    /// <summary>Create a single preserved raw record from JSON text.</summary>
    public static CddaSourceRecord ParseRecord(
        string json, string sourceName = "<memory>", int recordIndex = 0)
    {
        ArgumentNullException.ThrowIfNull(json);
        using var document = JsonDocument.Parse(json, CddaRecordParser.DocumentOptions);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty("type", out var type) ||
            type.ValueKind != JsonValueKind.String)
            throw new JsonException("Expected one JSON definition with a string 'type'.");
        return new CddaSourceRecord(sourceName, recordIndex, root.Clone());
    }

    public static async Task<IReadOnlyList<CddaSourceRecord>> LoadFileAsync(
        string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        await using var stream = File.OpenRead(filePath);
        using var document = await JsonDocument.ParseAsync(
            stream, CddaRecordParser.DocumentOptions, cancellationToken).ConfigureAwait(false);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new JsonException($"{filePath}: expected a JSON array of definitions.");

        var records = new List<CddaSourceRecord>();
        var index = 0;
        foreach (var element in document.RootElement.EnumerateArray())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (element.ValueKind != JsonValueKind.Object ||
                !element.TryGetProperty("type", out var type) ||
                type.ValueKind != JsonValueKind.String)
                throw new JsonException($"{filePath}[{index}]: expected an object with a type.");

            records.Add(new CddaSourceRecord(filePath, index++, element.Clone()));
        }
        return records;
    }

    public static async IAsyncEnumerable<CddaSourceRecord> LoadDirectoryAsync(
        string directoryPath,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        var root = Path.GetFullPath(directoryPath);
        if (!Directory.Exists(root)) throw new DirectoryNotFoundException(root);

        var directories = new Queue<string>();
        directories.Enqueue(root);

        while (directories.TryDequeue(out var directory))
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var file in Directory.EnumerateFiles(directory, "*.json", SearchOption.TopDirectoryOnly)
                         .OrderBy(file => file, StringComparer.Ordinal))
            {
                foreach (var record in await LoadFileAsync(file, cancellationToken).ConfigureAwait(false))
                    yield return record;
            }
            foreach (var subdirectory in Directory.EnumerateDirectories(directory)
                         .OrderBy(path => path, StringComparer.Ordinal))
                directories.Enqueue(subdirectory);
        }
    }

    /// <summary>Read a whole directory into preserved raw source records.</summary>
    public static async Task<IReadOnlyList<CddaSourceRecord>> LoadDirectoryRecordsAsync(
        string directoryPath, CancellationToken cancellationToken = default)
    {
        var items = new List<CddaSourceRecord>();
        await foreach (var record in LoadDirectoryAsync(directoryPath, cancellationToken))
            items.Add(record);
        return items;
    }

    /// <summary>
    /// Explicit opt-in path: source JSON -> item inheritance/patch resolution
    /// -> eager typed DTOs. The existing CddaJsonLoader APIs remain unchanged.
    /// </summary>
    public static async Task<CddaResolvedItemCatalog> LoadResolvedItemsAsync(
        string directoryPath,
        CddaItemResolver? resolver = null,
        CancellationToken cancellationToken = default)
    {
        var source = await LoadDirectoryRecordsAsync(directoryPath, cancellationToken)
            .ConfigureAwait(false);
        return (resolver ?? new CddaItemResolver()).Resolve(source);
    }
}
