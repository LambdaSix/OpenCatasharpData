using OpenCatasharpData.Models;

namespace OpenCatasharpData;

/// <summary>
/// Materialised results of a directory load. Definition objects are fully
/// deserialized; catalogue queries never parse JSON.
/// </summary>
public sealed class CddaDataCatalog
{
    private readonly Dictionary<string, IReadOnlyList<LoadedDefinition>> _byId;
    private readonly Dictionary<Type, IReadOnlyList<CddaDefinition>> _byExactType;
    private readonly Dictionary<string, IReadOnlyList<LoadedDefinition>> _bySourceFile;

    public CddaDataCatalog(IEnumerable<LoadedDefinition> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        Entries = Array.AsReadOnly(entries.ToArray());
        _byId = Entries
            .Where(entry => entry.Definition.IdString is not null)
            .GroupBy(entry => entry.Definition.IdString!, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<LoadedDefinition>)Array.AsReadOnly(group.ToArray()),
                StringComparer.Ordinal);

        _byExactType = Entries
            .GroupBy(entry => entry.Definition.GetType())
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<CddaDefinition>)Array.AsReadOnly(
                    group.Select(entry => entry.Definition).ToArray()));

        _bySourceFile = Entries
            .GroupBy(entry => entry.FilePath, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<LoadedDefinition>)Array.AsReadOnly(group.ToArray()),
                StringComparer.Ordinal);
    }

    public IReadOnlyList<LoadedDefinition> Entries { get; }

    /// <summary>
    /// Query typed results. Concrete sealed types use the exact-type index;
    /// base classes include all derived definitions in original source order.
    /// </summary>
    public IReadOnlyList<TDefinition> Get<TDefinition>() where TDefinition : CddaDefinition
    {
        if (typeof(TDefinition).IsSealed && _byExactType.TryGetValue(typeof(TDefinition), out var exact))
            return exact.Cast<TDefinition>().ToArray();

        return Entries.Select(entry => entry.Definition).OfType<TDefinition>().ToArray();
    }

    /// <summary>
    /// All items carrying a parsed subtype component (including composite items).
    /// No additional JSON parsing occurs during the query.
    /// </summary>
    public IReadOnlyList<ItemDefinition> GetItemsWith<TSlot>() where TSlot : class =>
        Entries.Select(entry => entry.Definition)
            .OfType<ItemDefinition>()
            .Where(item => item.GetSubtype<TSlot>() is not null)
            .ToArray();

    /// <summary>All definitions of this ID, including duplicates or mod patches.</summary>
    public IReadOnlyList<LoadedDefinition> FindById(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _byId.TryGetValue(id, out var matches) ? matches : [];
    }

    /// <summary>All definitions found in one source file.</summary>
    public IReadOnlyList<LoadedDefinition> FindBySourceFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        return _bySourceFile.TryGetValue(Path.GetFullPath(filePath), out var matches)
            ? matches
            : [];
    }
}
