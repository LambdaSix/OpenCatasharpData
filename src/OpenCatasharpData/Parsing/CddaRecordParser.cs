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

/// <summary>
/// One independently registered parser for an ITEM subtype. Implement this
/// interface for a new slot without changing the file loader or record parser.
/// </summary>
public interface ICddaItemSubtypeHandler
{
    string Subtype { get; }
    void Attach(JsonElement record, ItemDefinition item);
}

/// <summary>
/// Deserialize an ITEM slot once, then attach its strongly typed result.
/// Different handlers can coexist on the same item record.
/// </summary>
public sealed class JsonItemSubtypeHandler<TSlot> : ICddaItemSubtypeHandler where TSlot : class
{
    private readonly JsonSerializerOptions _options;

    public JsonItemSubtypeHandler(string subtype, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subtype);
        Subtype = subtype;
        _options = options ?? CddaRecordParser.JsonOptions;
    }

    public string Subtype { get; }

    public void Attach(JsonElement record, ItemDefinition item)
    {
        var slot = record.Deserialize<TSlot>(_options)
            ?? throw new JsonException($"Could not deserialize ITEM subtype {Subtype}.");
        item.SetSubtype(slot);
        if (slot is ArmorData armor)
            armor.Sections = item.Armor;
        if (slot is GunData gun)
        {
            gun.RangedDamage = item.RangedDamage;
            gun.BarrelLength = item.BarrelLength;
        }
        if (slot is AmmoData ammo) ammo.AmmoTypes = item.AmmoTypes;
        if (slot is MagazineData magazine) magazine.AmmoTypes = item.AmmoTypes;
        if (slot is GunModData gunMod) gunMod.BarrelLength = item.BarrelLength;
    }
}

/// <summary>Handle all ITEM subtypes as composable, eager components.</summary>
public sealed class CddaItemCompositionHandler : ICddaDefinitionHandler
{
    private readonly Dictionary<string, ICddaItemSubtypeHandler> _handlers;
    private readonly JsonSerializerOptions _options;

    public CddaItemCompositionHandler(
        IEnumerable<ICddaItemSubtypeHandler> handlers, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        _handlers = new Dictionary<string, ICddaItemSubtypeHandler>(StringComparer.OrdinalIgnoreCase);
        foreach (var handler in handlers)
        {
            if (!_handlers.TryAdd(handler.Subtype, handler))
                throw new ArgumentException($"Duplicate ITEM subtype handler: {handler.Subtype}");
        }
        _options = options ?? CddaRecordParser.JsonOptions;
    }

    public bool CanHandle(JsonElement record) =>
        CddaDefaultHandlers.TypeIs(record, "ITEM") ||
        CddaDefaultHandlers.TypeIs(record, "GENERIC") ||
        CddaDefaultHandlers.TypeIs(record, "BATTERY") ||
        _handlers.Keys.Any(subtype => CddaDefaultHandlers.TypeIs(record, subtype));

    public CddaDefinition Deserialize(JsonElement record)
    {
        var item = record.Deserialize<ItemDefinition>(_options)
            ?? throw new JsonException("Invalid ITEM definition.");
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Historical ITEM type discriminators can designate a subtype directly.
        if (_handlers.TryGetValue(item.Type, out var direct))
        {
            direct.Attach(record, item);
            seen.Add(direct.Subtype);
        }

        // Critically, every declared subtype is attached, not just the first.
        foreach (var subtype in item.Subtypes ?? [])
        {
            if (!seen.Add(subtype)) continue;
            if (_handlers.TryGetValue(subtype, out var handler))
                handler.Attach(record, item);
        }

        return item;
    }
}

/// <summary>Built-in record and subtype parser registries.</summary>
public static class CddaDefaultHandlers
{
    public static IReadOnlyList<ICddaItemSubtypeHandler> CreateItemSubtypeHandlers() =>
    [
        new JsonItemSubtypeHandler<ArmorData>("ARMOR"),
        new JsonItemSubtypeHandler<ComestibleData>("COMESTIBLE"),
        new JsonItemSubtypeHandler<ToolData>("TOOL"),
        new JsonItemSubtypeHandler<GunData>("GUN"),
        new JsonItemSubtypeHandler<AmmoData>("AMMO"),
        new JsonItemSubtypeHandler<MagazineData>("MAGAZINE"),
        new JsonItemSubtypeHandler<BookData>("BOOK"),
        new JsonItemSubtypeHandler<GunModData>("GUNMOD")
    ];

    public static IReadOnlyList<ICddaDefinitionHandler> Create(
        IEnumerable<ICddaItemSubtypeHandler>? itemHandlers = null) =>
    [
        new CddaItemCompositionHandler(itemHandlers ?? CreateItemSubtypeHandlers()),
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
}
