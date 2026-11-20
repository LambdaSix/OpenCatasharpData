using OpenCatasharpData.Models;

namespace OpenCatasharpData;

/// <summary>
/// Materialised results of a directory load. Objects are already fully
/// deserialized; queries do not re-read or parse the underlying JSON.
/// </summary>
public sealed class CddaDataCatalog
{
    private readonly Dictionary<string, IReadOnlyList<LoadedDefinition>> _byId;

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
    }

    public IReadOnlyList<LoadedDefinition> Entries { get; }

    public IReadOnlyList<TDefinition> Get<TDefinition>() where TDefinition : CddaDefinition =>
        Entries.Select(entry => entry.Definition).OfType<TDefinition>().ToArray();

    /// <summary>Returns all definitions of this ID, including duplicate/modded records.</summary>
    public IReadOnlyList<LoadedDefinition> FindById(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _byId.TryGetValue(id, out var matches) ? matches : [];
    }
}
