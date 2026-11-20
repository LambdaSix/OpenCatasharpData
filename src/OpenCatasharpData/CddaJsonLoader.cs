using System.Runtime.CompilerServices;
using System.Text.Json;
using OpenCatasharpData.Models;

namespace OpenCatasharpData;

/// <summary>
/// Deserializes raw CDDA JSON definitions. It does not resolve copy-from
/// inheritance or other game-specific patch operations.
/// </summary>
public static class CddaJsonLoader
{
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private static readonly Dictionary<string, Type> DefinitionTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["skill"] = typeof(SkillDefinition),
            ["material"] = typeof(MaterialDefinition),
            ["ITEM_CATEGORY"] = typeof(ItemCategoryDefinition),
            ["ammunition_type"] = typeof(AmmunitionTypeDefinition),
            ["recipe"] = typeof(RecipeDefinition),
            ["MONSTER"] = typeof(MonsterDefinition),
            ["ITEM"] = typeof(ItemDefinition),
            ["GENERIC"] = typeof(ItemDefinition),
            ["ARMOR"] = typeof(ItemDefinition),
            ["TOOL"] = typeof(ItemDefinition),
            ["GUN"] = typeof(ItemDefinition),
            ["AMMO"] = typeof(ItemDefinition),
            ["COMESTIBLE"] = typeof(ItemDefinition),
            ["BOOK"] = typeof(ItemDefinition),
            ["GUNMOD"] = typeof(ItemDefinition),
            ["MAGAZINE"] = typeof(ItemDefinition),
            ["BATTERY"] = typeof(ItemDefinition)
        };

    /// <summary>Load definitions from a JSON file.</summary>
    public static async Task<IReadOnlyList<CddaDefinition>> LoadFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        await using var stream = File.OpenRead(filePath);
        return await LoadAsync(stream, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Load definitions from an existing JSON stream.</summary>
    public static async Task<IReadOnlyList<CddaDefinition>> LoadAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var document = await JsonDocument.ParseAsync(
            stream, DocumentOptions, cancellationToken).ConfigureAwait(false);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Expected a top-level array of CDDA definitions.");
        }

        var definitions = new List<CddaDefinition>();
        var index = 0;

        foreach (var element in document.RootElement.EnumerateArray())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (element.ValueKind != JsonValueKind.Object
                || !element.TryGetProperty("type", out var typeElement)
                || typeElement.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(typeElement.GetString()))
            {
                throw new JsonException(
                    $"Definition at index {index} must be an object with a string 'type'.");
            }

            var typeName = typeElement.GetString()!;
            var modelType = DefinitionTypes.TryGetValue(typeName, out var knownType)
                ? knownType
                : typeof(UnknownDefinition);

            var definition = (CddaDefinition?)element.Deserialize(
                modelType, SerializerOptions);

            definitions.Add(definition
                ?? throw new JsonException($"Unable to deserialize definition at index {index}."));
            index++;
        }

        return definitions;
    }

    /// <summary>
    /// Recursively enumerate JSON files. Files are processed in path order,
    /// one at a time, so the entire directory is not buffered in memory.
    /// </summary>
    public static async IAsyncEnumerable<LoadedDefinition> LoadDirectoryAsync(
        string directoryPath,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        var root = Path.GetFullPath(directoryPath);
        if (!Directory.Exists(root))
        {
            throw new DirectoryNotFoundException(root);
        }

        foreach (var file in Directory.EnumerateFiles(
                     root, "*.json", SearchOption.AllDirectories)
                 .OrderBy(path => path, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var definitions = await LoadFileAsync(file, cancellationToken)
                .ConfigureAwait(false);

            foreach (var definition in definitions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return new LoadedDefinition(file, definition);
            }
        }
    }
}

/// <summary>Source filename alongside its deserialized definition.</summary>
public sealed record LoadedDefinition(string FilePath, CddaDefinition Definition);
