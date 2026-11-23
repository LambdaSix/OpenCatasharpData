# OpenCatasharpData

A small **.NET 10** class library for eagerly parsing [Cataclysm: Dark Days Ahead](https://github.com/LambdaSix/Cataclysm-DDA) definition JSON into typed C# DTOs using **System.Text.Json**, without third-party runtime dependencies.

## Getting started

Requires the .NET 10 SDK.

~~~bash
dotnet build OpenCatasharpData.slnx
dotnet run --project tests/OpenCatasharpData.SmokeTests
~~~

### Parse one record

~~~csharp
using OpenCatasharpData.Parsing;
using OpenCatasharpData.Models;

CddaDefinition definition = CddaRecordParser.Default.Parse(
    """{"type":"ITEM","subtypes":["COMESTIBLE"],"id":"apple","calories":95}""");

var item = (ItemDefinition)definition;
Console.WriteLine($"{item.IdString}: {item.ComestibleData?.Calories}");
~~~

Single-record overloads accept UTF-8 bytes and JsonElement too. Known fields are converted to typed values at deserialization time.

### Load files or a complete directory

~~~csharp
using OpenCatasharpData;
using OpenCatasharpData.Models;

// A file is typically an array of source definitions.
var definitions = await CddaJsonLoader.LoadFileAsync(
    "/path/to/Cataclysm-DDA/data/json/items/generic.json");

// Fully materialise and index a recursively loaded directory.
CddaDataCatalog catalog = await CddaJsonLoader.LoadDirectoryCatalogAsync(
    "/path/to/Cataclysm-DDA/data/json/items");

IReadOnlyList<ItemDefinition> foods = catalog.GetItemsWith<ComestibleData>();
IReadOnlyList<LoadedDefinition> matches = catalog.FindById("apple_cider");

// Or stream one file at a time.
await foreach (var record in CddaJsonLoader.LoadDirectoryAsync(
    "/path/to/Cataclysm-DDA/data/json/items"))
    Console.WriteLine($"{record.Definition.IdString} in {record.FilePath}");
~~~

Duplicate IDs remain visible in FindById, allowing later mod/patch resolution.

### Register an independent parser handler

Handlers implement ICddaDefinitionHandler and are tried in registration order. The generic JsonDefinitionHandler<T> requires only a predicate and a DTO type:

~~~csharp
using OpenCatasharpData.Parsing;
using OpenCatasharpData.Models;

var parser = new CddaRecordParser([
    new JsonDefinitionHandler<MyDefinition>(json =>
        CddaDefaultHandlers.TypeIs(json, "MY_CUSTOM_TYPE")),
    .. CddaDefaultHandlers.Create()
]);

// Pass the same parser into either single-record parsing or the loaders.
var catalog = await CddaJsonLoader.LoadDirectoryCatalogAsync(
    "/path/to/data/json", parser: parser);

// MyDefinition must inherit CddaDefinition.
~~~

No loader changes are necessary to register new handlers or new DTO types.

## Composable item subtypes and complete comestibles

An ITEM definition is always an ItemDefinition with **one eagerly parsed data
component for each recognised declared subtype**. It is not classified into a
single derived class anymore. Combined ARMOR/TOOL/COMESTIBLE items expose all
three typed slots; unknown subtype names remain in the Subtypes list.

The built-in slot properties are ArmorData, ComestibleData, ToolData, GunData,
AmmoData, MagazineData, BookData and GunModData. ItemDefinition.GetSubtype<T>()
works for built-in and custom slots, with no deferred JSON parsing.

~~~csharp
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

var item = (ItemDefinition)CddaRecordParser.Default.Parse(
    """
    {"type":"ITEM","id":"food_armor","subtypes":["COMESTIBLE","ARMOR"],
     "calories":150,"healthy":-2,
     "armor":[{"covers":["torso"],"coverage":75}]}
    """);

decimal? calories = item.ComestibleData?.Calories;   // 150
decimal? health = item.ComestibleData?.Health;       // -2
decimal? coverage = item.ArmorData?.Sections?[0].Coverage; // 75
~~~

Each slot has an independent ICddaItemSubtypeHandler. You can register
new subtype handlers without editing the record parser, JSON file loader or
directory loader:

~~~csharp
var handlers = CddaDefaultHandlers.CreateItemSubtypeHandlers().ToList();
handlers.Add(new JsonItemSubtypeHandler<MyCustomSlot>("MY_CUSTOM_SUBTYPE"));
var parser = new CddaRecordParser(CddaDefaultHandlers.Create(handlers));
~~~

Your MyCustomSlot class can use System.Text.Json attributes. The parser eagerly
deserialises it and ItemDefinition.GetSubtype<MyCustomSlot>() retrieves it.
To query a materialised directory catalogue by slot, use
catalog.GetItemsWith<ComestibleData>() rather than by runtime subclass.

ComestibleData covers the documented consumption, nutrition, and food
metadata: type, calories, fun, quench, spoilage, vitamins, stimulation,
sleepiness, health, addiction potential and addiction types, monotony, serving
tools, stack sizes, seasonings, freezing point, cooking/eating equivalence,
parasites, contamination, rot spawn, smoking results and pet-food categories.
Consumption EOCs can be referenced by ID or declared inline; known metadata is
typed and unmodelled inline EOC details are retained as extension properties.
Tool qualities and vitamin quantities remain independently typed.

This refactor **intentionally changes the API**: use ItemDefinition and its
component properties instead of GenericItemDefinition, ArmorItemDefinition,
ComestibleItemDefinition, etc. Common fields (including material, pockets and
ammo types) remain on ItemDefinition.

Shared values use eager System.Text.Json converters:

- CddaText for string or translatable-object names
- CddaVolume, CddaLength, CddaMass and CddaDuration for dimension-safe quantities
- CddaScalar for values not yet classified into dedicated dimensions (including currency and power)
- CddaMaterialPart for string and object material entries
- CddaQuality and CddaVitamin for mixed-shape qualities and vitamins
- CddaArmorSection and CddaPocket for nested typed objects
- String or string-array ammunition types as List<string>

Other supported top-level DTOs remain: SkillDefinition, MaterialDefinition, ItemCategoryDefinition, AmmunitionTypeDefinition, RecipeDefinition and MonsterDefinition. Unknown definitions use UnknownDefinition.

## Expanded armour and pocket DTOs

Every ITEM declaring ARMOR exposes its eager `ArmorData` component alongside
the item's shared `Armor` and `PocketData` lists. This works for composite
items, including ARMOR+TOOL and ARMOR+BOOK, with no first-match data loss.

~~~csharp
var item = (ItemDefinition)CddaRecordParser.Default.Parse(json);

bool? sided = item.ArmorData?.Sided;
decimal? meleeCoverage = item.ArmorData?.Sections?[0].MeleeCoverage;
decimal? rangedCoverage = item.ArmorData?.Sections?[0].RangedCoverage;
string? layerBreathability = item.ArmorData?.Sections?[0].Breathability;

decimal? maximumPocketLengthMm =
    item.PocketData?[0].MaxItemLength?.Millimetres;
decimal? minimumPocketVolumeMl =
    item.PocketData?[0].MinimumItemVolume?.Millilitres;
~~~

ArmorData includes sided equipment, power-armour status, warmth, thickness,
environmental protection, covered parts, overall/melee/ranged/vital coverage,
encumbrance and maximum encumbrance, valid clothing modifications, maximum
worn count, destroyed-item transformations and related damage descriptions.

Each CddaArmorSection adds melee/ranged coverage, breathability, rigid-layer
constraints, volume-based encumbrance modifiers and typed activity-noise
volume/chance. CddaPocket adds magazine defaults, minimum lengths/volumes,
maximum individual item volume, open/airtight/fire-protected pocket flags,
ripoff and extra encumbrance values, multipliers, seal metadata and activity
noise. Existing `CddaLength`, `CddaVolume` and `CddaMass` types are used
for unit-bearing limits.

**Source values are not game-calculated defaults.** Omitted fields remain
nullable. Unknown nested keys are still retained through JsonExtensionData.
The `energy_shield_max_hp` field is retained as a JsonElement because it can
carry special shape semantics requiring separate modelling.

The schema scan and pinned-CDDA deserialization probe run on every relevant
change. BenchmarkDotNet includes a new ParseArmorDetailed scenario, without
changing the input to the existing ParseArmor baseline.

## Typed physical measurements

Dimension-aware value types replace unstructured CddaScalar values where the JSON schema defines physical units. They are readonly record structs with System.Text.Json converters and eager conversions. A duration's m suffix means **minutes**, while a length's m suffix means **metres**.

| Value type | Source units | Canonical quantity |
|---|---|---|
| CddaVolume | ml, L; supported singular/plural names | Millilitres |
| CddaLength | mm, cm, m, km; supported singular/plural names | Millimetres |
| CddaMass | mg, g, kg; supported singular/plural names | Milligrams |
| CddaDuration | ms, s, turns, minutes, hours, days, weeks | Seconds |

Example using loaded DTOs:

~~~csharp
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

var item = (ItemDefinition)CddaRecordParser.Default.Parse(
    """{"type":"ITEM","id":"mug","weight":"1.5 kg","volume":"250 ml",
        "longest_side":"15 cm"}""");

decimal? weightInGrams = item.Weight?.Grams;       // 1500
decimal? volumeInLitres = item.Volume?.Litres;     // 0.25
decimal? lengthInMm = item.LongestSide?.Millimetres; // 150

bool equals = CddaVolume.FromLitres(1m) == CddaVolume.FromMillilitres(1000m);
~~~

Other migrated fields include integral_weight/volume, nested pocket weight, volume,
length and magazine-well dimensions, ammunition damage barrel lengths,
ComestibleData.SpoilsIn, BookData.ReadingTime and
GunModData.InstallTime.

The source spelling is preserved during JSON serialisation (for example "0.25 L"
will not turn into "250 ml"). Values created through typed factory methods use
canonical suffixes. An unqualified JSON number remains an unqualified number;
its HasExplicitUnit flag is false and normalised conversions return null instead
of guessing an implicit game unit. Unsupported or dimensionally incorrect units
throw JsonException. Equality of explicit measurements compares their normalised
values, so 1 L equals 1000 ml.

This is an **API change**: these DTO properties now expose distinct measurement
value types, not CddaScalar. CddaScalar remains for currency, charge rate and
vitamin amount values until their own semantics are modelled.

## Item schema coverage and validation

The automated [schema-coverage workflow](.github/workflows/schema-coverage.yml) scans a **pinned** version of
[LambdaSix/Cataclysm-DDA](https://github.com/LambdaSix/Cataclysm-DDA) (commit `e262adb299a7613b4aedc5f12c08fe0413c56a84`).
It inventories field names, observed shapes, subtype frequency and missing nested DTO fields.

- [Baseline coverage report](reports/item-schema-coverage.baseline.md): 85.4% of authored top-level ITEM field occurrences mapped to a DTO property.
- [Post-expansion coverage report](reports/item-schema-coverage.after.md): 96.9% of authored top-level ITEM field occurrences mapped (pre-composition baseline).
- Real-data validation: **489/489 JSON files and 10,525/10,525 definitions parsed** successfully from the pinned `data/json/items` directory.

The percentage measures **declared field-name mapping**, not semantic correctness or inheritance resolution.
The probe verifies that all the pinned core item files deserialize without exceptions.
Mods and other areas of the game data tree are not part of this particular probe.

Common item fields now eagerly typed include `melee_damage`, both representations of `to_hit`,
`variants`, `use_action`/`tick_action`/`drop_action`, `ranged_damage`, `looks_like`, `longest_side`,
`replace_materials`, `techniques`, `faults`, `conditional_names`, and nested armour/pocket properties.
Gun modes and mod slots, ammunition/shot damage with barrel-length curves, numeric or unit-bearing
vitamins, and tuple/object tool qualities have dedicated DTOs/converters.

To reproduce against a local copy of the fork:

~~~bash
python3 tools/schema_coverage.py --data-dir /path/to/Cataclysm-DDA/data/json/items \
  --model-dir src/OpenCatasharpData/Models --output artifacts/item-schema-coverage.md
dotnet run --project tests/OpenCatasharpData.SmokeTests -c Release -- \
  --scan-dir /path/to/Cataclysm-DDA/data/json/items
~~~

See [coverage scanner guidance](tools/SCHEMA_COVERAGE.md) for caveats and re-baselining.

## Source versus resolved ITEM definitions

A new opt-in CddaSourceLoader + CddaItemResolver layer preserves the original
JSON and materialises a **separate resolved, eagerly typed** ItemDefinition.
It handles common ITEM copy-from, numeric relative/proportional operations,
container extend/delete, abstract templates and chained inheritance.
Missing parents, cycles, ambiguous identities and unsupported patch shapes
produce explicit errors instead of silently losing data.

The existing raw CddaJsonLoader APIs remain unchanged. See
[source-resolution documentation](doc/ITEM_RESOLUTION.md) for API examples,
limitations and additional BenchmarkDotNet coverage.

## Scope and limitations

- This reads **raw source definitions**, not final resolved game data. copy-from, extend, delete, relative, and proportional are preserved but **not applied**.
- A composite item has a typed slot for each recognised declared subtype. Unrecognised fields and unsupported subtype declarations remain available for future modelling.
- Unmodelled fields are preserved via JsonExtensionData. Known model fields are eagerly deserialized only once, as the record is read.
- Recognised physical units are normalised by their dedicated value types, but unqualified numeric fields are deliberately not assigned an implied unit. Other scalar domains (including prices and power) remain CddaScalar.
- Files are expected to contain arrays of definitions. Directory enumeration processes files in path order and returns source paths.
- Tests use reduced, representative source records from the fork. **The entire data tree has not yet been exhaustively validated.**

## License

MIT; see LICENSE.
