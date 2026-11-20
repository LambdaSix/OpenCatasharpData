using System.Text.Json;
using OpenCatasharpData.Models;

namespace OpenCatasharpData.Parsing;

/// <summary>A handler that matches and eagerly deserializes one kind of definition.</summary>
public interface ICddaDefinitionHandler
{
    bool CanHandle(JsonElement record);
    CddaDefinition Deserialize(JsonElement record);
}

/// <summary>
/// Reusable System.Text.Json handler; its match predicate is independent of the DTO.
/// Register custom predicates/DTOs without changing the file or directory loaders.
/// </summary>
public sealed class JsonDefinitionHandler<TDefinition> : ICddaDefinitionHandler
    where TDefinition : CddaDefinition
{
    private readonly Func<JsonElement, bool> _canHandle;
    private readonly JsonSerializerOptions _options;

    public JsonDefinitionHandler(Func<JsonElement, bool> canHandle, JsonSerializerOptions? options = null)
    {
        _canHandle = canHandle ?? throw new ArgumentNullException(nameof(canHandle));
        _options = options ?? CddaRecordParser.JsonOptions;
    }

    public bool CanHandle(JsonElement record) => _canHandle(record);

    public CddaDefinition Deserialize(JsonElement record) =>
        record.Deserialize<TDefinition>(_options)
        ?? throw new JsonException($"Could not deserialize {typeof(TDefinition).Name}.");
}

/// <summary>
/// Single-record entry point. Each definition is deserialized exactly once,
/// by the first matching handler. Unknown types use UnknownDefinition.
/// </summary>
public sealed class CddaRecordParser
{
    internal static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private readonly IReadOnlyList<ICddaDefinitionHandler> _handlers;

    public CddaRecordParser(IEnumerable<ICddaDefinitionHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        _handlers = handlers.ToArray();
    }

    public static CddaRecordParser Default { get; } = new(CddaDefaultHandlers.Create());

    public CddaDefinition Parse(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        using var document = JsonDocument.Parse(json, DocumentOptions);
        return Parse(document.RootElement);
    }

    public CddaDefinition Parse(ReadOnlySpan<byte> utf8Json)
    {
        using var document = JsonDocument.Parse(utf8Json.ToArray(), DocumentOptions);
        return Parse(document.RootElement);
    }

    public CddaDefinition Parse(JsonElement record)
    {
        if (record.ValueKind != JsonValueKind.Object
            || !record.TryGetProperty("type", out var type)
            || type.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(type.GetString()))
            throw new JsonException("A CDDA definition must be an object with a string 'type'.");

        CddaDefinition? definition = null;
        foreach (var handler in _handlers)
        {
            if (!handler.CanHandle(record)) continue;
            definition = handler.Deserialize(record);
            break;
        }

        definition ??= record.Deserialize<UnknownDefinition>(JsonOptions)
            ?? throw new JsonException("Could not deserialize unknown definition.");

        // Populate once, while JSON is being parsed, not on DTO property access.
        if (record.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
            definition.IdString = id.GetString();

        return definition;
    }
}

/// <summary>Built-in routing rules. CDDA uses type=ITEM with a subtypes array.</summary>
public static class CddaDefaultHandlers
{
    private static readonly HashSet<string> KnownItemSubtypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "ARMOR", "COMESTIBLE", "TOOL", "GUN", "AMMO",
        "MAGAZINE", "BOOK", "GUNMOD"
    };

    public static IReadOnlyList<ICddaDefinitionHandler> Create() =>
    [
        new JsonDefinitionHandler<ArmorItemDefinition>(record => Item(record, "ARMOR")),
        new JsonDefinitionHandler<ComestibleItemDefinition>(record => Item(record, "COMESTIBLE")),
        new JsonDefinitionHandler<ToolItemDefinition>(record => Item(record, "TOOL")),
        new JsonDefinitionHandler<GunItemDefinition>(record => Item(record, "GUN")),
        new JsonDefinitionHandler<AmmoItemDefinition>(record => Item(record, "AMMO")),
        new JsonDefinitionHandler<MagazineItemDefinition>(record => Item(record, "MAGAZINE")),
        new JsonDefinitionHandler<BookItemDefinition>(record => Item(record, "BOOK")),
        new JsonDefinitionHandler<GunModItemDefinition>(record => Item(record, "GUNMOD")),
        new JsonDefinitionHandler<GenericItemDefinition>(record =>
            TypeIs(record, "ITEM") || TypeIs(record, "GENERIC") || TypeIs(record, "BATTERY")),
        new JsonDefinitionHandler<SkillDefinition>(record => TypeIs(record, "skill")),
        new JsonDefinitionHandler<MaterialDefinition>(record => TypeIs(record, "material")),
        new JsonDefinitionHandler<ItemCategoryDefinition>(record => TypeIs(record, "ITEM_CATEGORY")),
        new JsonDefinitionHandler<AmmunitionTypeDefinition>(record => TypeIs(record, "ammunition_type")),
        new JsonDefinitionHandler<RecipeDefinition>(record => TypeIs(record, "recipe")),
        new JsonDefinitionHandler<MonsterDefinition>(record => TypeIs(record, "MONSTER"))
    ];

    public static bool TypeIs(JsonElement record, string type) =>
        record.ValueKind == JsonValueKind.Object
        && record.TryGetProperty("type", out var value)
        && value.ValueKind == JsonValueKind.String
        && string.Equals(value.GetString(), type, StringComparison.OrdinalIgnoreCase);

    private static bool Item(JsonElement record, string subtype)
    {
        if (TypeIs(record, subtype)) return true;
        if (!TypeIs(record, "ITEM")
            || !record.TryGetProperty("subtypes", out var subtypes)
            || subtypes.ValueKind != JsonValueKind.Array) return false;

        // First recognised subtype wins, reflecting the declared order.
        foreach (var value in subtypes.EnumerateArray())
        {
            if (value.ValueKind != JsonValueKind.String) continue;
            var declared = value.GetString();
            if (declared is null || !KnownItemSubtypes.Contains(declared)) continue;
            return string.Equals(declared, subtype, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
