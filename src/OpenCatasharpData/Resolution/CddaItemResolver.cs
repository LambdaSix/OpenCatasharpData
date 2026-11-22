using System.Text.Json;
using System.Text.Json.Nodes;
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

namespace OpenCatasharpData.Resolution;

/// <summary>
/// A user-actionable error while resolving a source item. Resolution never
/// falls back silently to an incomplete or otherwise misleading DTO.
/// </summary>
public sealed class CddaResolutionException : Exception
{
    public CddaResolutionException(CddaSourceRecord source, string message, Exception? inner = null)
        : base($"{source}: {message}", inner)
    {
        SourceRecord = source;
    }

    public CddaSourceRecord SourceRecord { get; }
}

/// <summary>
/// Resolved materialised ITEM plus its immutable original source and flattened
/// JSON. EffectiveSubtypes includes inherited slots; Definition.Subtypes
/// contains only subtypes explicitly authored on this source record.
/// </summary>
public sealed record CddaResolvedItem(
    CddaSourceRecord Source,
    ItemDefinition Definition,
    JsonElement EffectiveJson,
    IReadOnlyList<string> EffectiveSubtypes);

/// <summary>Fully materialised items, separate from source records and abstract templates.</summary>
public sealed class CddaResolvedItemCatalog
{
    private readonly Dictionary<string, CddaResolvedItem> _byId;

    public CddaResolvedItemCatalog(
        IEnumerable<CddaResolvedItem> items,
        IEnumerable<CddaResolvedItem> templates,
        IEnumerable<CddaSourceRecord> sourceRecords)
    {
        Items = Array.AsReadOnly(items.ToArray());
        Templates = Array.AsReadOnly(templates.ToArray());
        SourceRecords = Array.AsReadOnly(sourceRecords.ToArray());
        _byId = Items.ToDictionary(entry => entry.Definition.IdString!, StringComparer.Ordinal);
    }

    public IReadOnlyList<CddaResolvedItem> Items { get; }
    public IReadOnlyList<CddaResolvedItem> Templates { get; }
    public IReadOnlyList<CddaSourceRecord> SourceRecords { get; }

    public CddaResolvedItem? FindById(string id) =>
        _byId.TryGetValue(id, out var value) ? value : null;

    public IReadOnlyList<CddaResolvedItem> GetItemsWith<TSlot>() where TSlot : class =>
        Items.Where(entry => entry.Definition.GetSubtype<TSlot>() is not null).ToArray();
}

/// <summary>
/// Strict, opt-in first-pass resolver for core type=ITEM sources. Inheritance
/// and source patch directives are applied before the existing independent
/// JSON record parser eagerly materialises all applicable subtype slots.
///
/// Supports parent lookup, abstract templates, chained inheritance,
/// explicit field replacement, numeric relative/proportional updates,
/// nested numeric objects, and array extend/delete. Unsupported shape
/// combinations raise CddaResolutionException rather than dropping data.
///
/// This is intentionally NOT a C++ gameplay-equivalent resolver for every
/// JSON discriminator or CDDA-specific inheritance rule.
/// </summary>
public sealed class CddaItemResolver
{
    private readonly CddaRecordParser _parser;

    public CddaItemResolver(CddaRecordParser? parser = null) =>
        _parser = parser ?? CddaRecordParser.Default;

    public CddaResolvedItemCatalog Resolve(IEnumerable<CddaSourceRecord> sourceRecords)
    {
        ArgumentNullException.ThrowIfNull(sourceRecords);
        var sources = sourceRecords.ToArray();
        var items = sources.Where(source =>
                string.Equals(source.Type, "ITEM", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var keys = new Dictionary<string, CddaSourceRecord>(StringComparer.Ordinal);
        foreach (var source in items)
        {
            var id = source.Id;
            var abstractId = source.AbstractId;
            if ((id is null) == (abstractId is null))
                throw new CddaResolutionException(
                    source, "ITEM must declare exactly one string 'id' or 'abstract'.");

            var key = id ?? abstractId!;
            if (!keys.TryAdd(key, source))
                throw new CddaResolutionException(
                    source, $"Duplicate item identifier '{key}' also found at {keys[key]}. " +
                            "Resolve duplicate/mod load order explicitly rather than guessing.");
        }

        var context = new ResolutionContext(keys);
        var resolved = new List<CddaResolvedItem>();
        var templates = new List<CddaResolvedItem>();

        foreach (var source in items)
        {
            var effective = context.Resolve(source);
            using var doc = JsonDocument.Parse(effective.ToJsonString());
            if (_parser.Parse(doc.RootElement) is not ItemDefinition item)
                throw new CddaResolutionException(source, "Configured parser did not produce ItemDefinition.");

            // The synthetic effective JSON carries *all* inherited slots so
            // their DTOs were eagerly attached. Restore source-only declarations
            // on the returned DTO; EffectiveSubtypes names the complete set.
            item.Subtypes = ReadStringList(source.RawJson, "subtypes");
            var resolvedItem = new CddaResolvedItem(
                source, item, doc.RootElement.Clone(),
                Array.AsReadOnly(ReadStringList(effective, "subtypes")?.ToArray() ?? []));

            if (source.AbstractId is null) resolved.Add(resolvedItem);
            else templates.Add(resolvedItem);
        }

        return new CddaResolvedItemCatalog(resolved, templates, sources);
    }

    private static List<string>? ReadStringList(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind != JsonValueKind.Array)
            throw new JsonException($"Expected '{name}' to be a string array.");
        return value.EnumerateArray().Select(entry =>
            entry.ValueKind == JsonValueKind.String
                ? entry.GetString()!
                : throw new JsonException($"Expected string member in '{name}'.")).ToList();
    }

    private static List<string>? ReadStringList(JsonObject root, string name)
    {
        if (root[name] is not JsonArray values) return null;
        return values.Select(entry =>
            entry is JsonValue ? entry.GetValue<string>() :
                throw new JsonException($"Invalid subtype member in '{name}'.")).ToList();
    }

    private sealed class ResolutionContext
    {
        private static readonly HashSet<string> SourceDirectives = new(StringComparer.Ordinal)
        {
            "copy-from", "extend", "delete", "relative", "proportional"
        };

        private readonly IReadOnlyDictionary<string, CddaSourceRecord> _records;
        private readonly Dictionary<CddaSourceRecord, JsonObject> _cached =
            new(ReferenceEqualityComparer.Instance);
        private readonly HashSet<CddaSourceRecord> _visiting =
            new(ReferenceEqualityComparer.Instance);
        private readonly List<string> _path = [];

        public ResolutionContext(IReadOnlyDictionary<string, CddaSourceRecord> records) =>
            _records = records;

        public JsonObject Resolve(CddaSourceRecord source)
        {
            if (_cached.TryGetValue(source, out var cached))
                return (JsonObject)cached.DeepClone();

            var label = source.Id ?? source.AbstractId!;
            if (!_visiting.Add(source))
                throw new CddaResolutionException(source,
                    $"Cyclic copy-from chain detected: {string.Join(" -> ", _path.Append(label))}.");

            _path.Add(label);
            try
            {
                var child = JsonNode.Parse(source.RawJson.GetRawText()) as JsonObject
                    ?? throw new CddaResolutionException(source, "ITEM must be a JSON object.");

                JsonObject effective;
                if (source.CopyFrom is { Length: > 0 } parentId)
                {
                    if (!_records.TryGetValue(parentId, out var parent))
                        throw new CddaResolutionException(
                            source, $"Missing copy-from parent '{parentId}'. " +
                                    "Include the parent definition in the source dataset.");

                    effective = Resolve(parent);

                    // Relative / proportional updates are calculated against
                    // inherited values, before explicit field replacements.
                    CddaItemPatch.ApplyNumeric(effective, child["relative"], false, source);
                    CddaItemPatch.ApplyNumeric(effective, child["proportional"], true, source);
                }
                else
                {
                    if (child.ContainsKey("relative") || child.ContainsKey("proportional"))
                        throw new CddaResolutionException(source,
                            "Relative/proportional patches require a copy-from parent.");
                    effective = new JsonObject();
                }

                var inherited = ReadStringList(effective, "subtypes") ?? [];
                var declared = ReadStringList(source.RawJson, "subtypes") ?? [];
                var effectiveSubtypes = inherited.Concat(declared)
                    .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

                effective.Remove("id");
                effective.Remove("abstract");
                effective.Remove("subtypes");

                foreach (var property in child)
                {
                    if (SourceDirectives.Contains(property.Key) ||
                        property.Key == "subtypes") continue;
                    effective[property.Key] = property.Value?.DeepClone();
                }

                if (effectiveSubtypes.Length > 0)
                {
                    var array = new JsonArray();
                    foreach (var subtype in effectiveSubtypes) array.Add(subtype);
                    effective["subtypes"] = array;
                }

                CddaItemPatch.ApplyContainers(effective, child["extend"], false, source);
                CddaItemPatch.ApplyContainers(effective, child["delete"], true, source);

                _cached[source] = (JsonObject)effective.DeepClone();
                return effective;
            }
            catch (CddaResolutionException) { throw; }
            catch (Exception error) when (error is JsonException or InvalidOperationException or ArgumentException or OverflowException)
            {
                throw new CddaResolutionException(source, error.Message, error);
            }
            finally
            {
                _path.RemoveAt(_path.Count - 1);
                _visiting.Remove(source);
            }
        }
    }
}
