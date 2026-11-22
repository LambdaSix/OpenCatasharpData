# Source versus resolved CDDA data

The raw source loader and ITEM resolver are **opt-in** and intentionally
separate from the existing eagerly typed CddaJsonLoader, which continues to
return definitions exactly as written without applying inheritance.

## Usage

~~~csharp
using OpenCatasharpData.Models;
using OpenCatasharpData.Resolution;

// Read source files without losing copy-from, patches or original JSON.
var source = await CddaSourceLoader.LoadDirectoryRecordsAsync(
    "/path/to/Cataclysm-DDA/data/json/items");

// Strictly resolve all included ITEM records, then produce eagerly typed DTOs.
var catalog = new CddaItemResolver().Resolve(source);

var item = catalog.FindById("556");
var damage = item?.Definition.AmmoData?.Damage?.Amount;
var original = item?.Source.RawJson;  // Original unmodified JSON.
var effective = item?.EffectiveJson;  // Flattened, directives removed.

// ItemDefinition.Subtypes = the list declared on the child source.
// EffectiveSubtypes = inherited and declared slots, all materialised.
var slotNames = item?.EffectiveSubtypes;
~~~

Alternatively, the convenience API reads the directory and resolves it:

~~~csharp
var result = await CddaSourceLoader.LoadResolvedItemsAsync(
    "/path/to/my-complete-item-data-directory");
var consumables = result.GetItemsWith<ComestibleData>();
~~~

CddaSourceLoader.ParseRecord(json) supports constructing an individual
raw source object. Use new CddaItemResolver(myCddaRecordParser) to pass
the same independent/custom handler registry used for raw DTO loading.

## Result model

- CddaResolvedItemCatalog.Items contains concrete ITEM definitions.
- Templates contains abstract definitions, used only as parents.
- SourceRecords contains the raw input records supplied to the resolver,
  including non-ITEM definitions (they are never resolved by this ITEM resolver).
- CddaResolvedItem.Source stores file path, array index and cloned original JSON.
- CddaResolvedItem.Definition is the final eagerly deserialised ItemDefinition
  with all recognised subtype components attached.
- EffectiveJson is the flattened JSON used to construct that DTO.
- EffectiveSubtypes includes the inherited slots. The DTO's Subtypes retains
  only the source record's declared subtypes list.

Input records are never mutated. Source identity and inheritance provenance
remain available even if a resolved DTO is changed.

## Implemented ITEM inheritance rules

The first version follows the common ITEM rules described in your fork's
doc/JSON/JSON_INHERITANCE.md:

- copy-from with abstract or concrete parents and chained inheritance.
- Missing fields inherit from the parent; explicit child fields replace fields.
- The child's declared subtypes is **not** inherited as a declared list, but
  the effective subtype slots are inherited so composed components still load.
- relative adds numeric offsets to inherited numbers, including nested
  numeric fields such as damage.amount. Matching discriminator strings
  (e.g. damage_type) are retained.
- proportional multiplies inherited numbers and simple unit-bearing strings
  (e.g. "1060 g", "500 ml", "100 USD").
- extend appends to arrays and merges nested objects.
- delete removes matching JSON array entries, with nested key handling.
- Duplicated item identifiers, missing parents, cycles and unsupported patch
  shapes throw CddaResolutionException with source details.

These transformations are performed on a **fresh JSON tree**, then the
existing independent parser handlers materialise fully typed DTOs.

## Important limits

**This is a conservative source-data resolver, not a complete reimplementation
of the CDDA C++ JSON loaders.**

- Scope is type: ITEM; other source discriminators are retained untouched.
- Item identity keys must be unique across the supplied dataset. Conflicting
  mod overrides/load-order updates are **rejected**, not guessed at.
- The set of source records must include referenced parents, including
  parent templates located outside the chosen directory.
- Numeric patch support is deliberately restricted to shapes with unambiguous
  arithmetic. Compound money strings such as "2 USD 80 cent" and specialised
  C++ patch semantics are not resolved yet; unsupported shapes fail explicitly.
- This layer does not apply game-side defaults or validate that the resulting
  content has equivalent gameplay semantics to CDDA.
- Unmodelled leaf properties survive in the effective JSON and extension data.
- The existing catalog and raw-loader entry points are unchanged; callers
  opt into source resolution explicitly.
- No lazy parsing takes place in the resulting item DTOs. Parsing source JSON
  trees is a separate ingestion/resolution operation, not a DTO accessor.

The pinned GitHub CI probe checks all **489 source item files** for raw
deserialization, plus a real fork 223 -> 556 ITEM inheritance case.
It does *not* claim that every mod or every ITEM copy-from chain in the full
game tree can yet be resolved.

## Performance

The BenchmarkDotNet project includes ResolutionBenchmarks for resolving 100
already prepared raw source items and parsing a single source record.
Its measurements are separate from the existing file and catalogue benchmarks
so the impact of this opt-in transformation can be evaluated independently.

Run with:

~~~bash
dotnet run --project benchmarks/OpenCatasharpData.Benchmarks -c Release -- \
  --filter '*ResolutionBenchmarks*'
~~~
