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

var food = (ComestibleItemDefinition)definition;
Console.WriteLine($"{food.IdString}: {food.Calories}");
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

IReadOnlyList<ComestibleItemDefinition> foods = catalog.Get<ComestibleItemDefinition>();
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

## Typed item models

CDDA uses a top-level discriminator of type=ITEM with a subtypes array rather than always using item subtype names in type. Handlers select the **first recognised subtype in source order**, ignoring unknown subtype names. The base ItemDefinition contains shared and composite-item properties such as armor, pocket_data and ammo_type.

| DTO | CDDA subtype | Selected eager properties |
|---|---|---|
| GenericItemDefinition | no recognised subtype | name, material, weight, volume, flags, qualities |
| ArmorItemDefinition | ARMOR | armor coverage, encumbrance, material layers, warmth |
| ComestibleItemDefinition | COMESTIBLE | calories, quench, fun, vitamins, spoilage |
| ToolItemDefinition | TOOL | charges and charge consumption |
| GunItemDefinition | GUN | skill, dispersion, handling, reload, durability |
| AmmoItemDefinition | AMMO | count, stack size, casing, range |
| MagazineItemDefinition | MAGAZINE | capacity, ammo types, reload time, pockets |
| BookItemDefinition | BOOK | skill, intelligence, chapters, reading time |
| GunModItemDefinition | GUNMOD | location, targets, modifiers, installation time |

Shared values use eager System.Text.Json converters:

- CddaText for string or translatable-object names
- CddaScalar for numeric or unit-bearing string values
- CddaMaterialPart for string and object material entries
- CddaNamedAmount for qualities and vitamins
- CddaArmorSection and CddaPocket for nested typed objects
- String or string-array ammunition types as List<string>

Other supported top-level DTOs remain: SkillDefinition, MaterialDefinition, ItemCategoryDefinition, AmmunitionTypeDefinition, RecipeDefinition and MonsterDefinition. Unknown definitions use UnknownDefinition.

## Item schema coverage and validation

The automated [schema-coverage workflow](.github/workflows/schema-coverage.yml) scans a **pinned** version of
[LambdaSix/Cataclysm-DDA](https://github.com/LambdaSix/Cataclysm-DDA) (commit `e262adb299a7613b4aedc5f12c08fe0413c56a84`).
It inventories field names, observed shapes, subtype frequency and missing nested DTO fields.

- [Baseline coverage report](reports/item-schema-coverage.baseline.md): 85.4% of authored top-level ITEM field occurrences mapped to a DTO property.
- [Post-expansion coverage report](reports/item-schema-coverage.after.md): 96.9% of authored top-level ITEM field occurrences mapped.
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

## Scope and limitations

- This reads **raw source definitions**, not final resolved game data. copy-from, extend, delete, relative, and proportional are preserved but **not applied**.
- A composite item has one concrete DTO, determined by the first recognised subtype; its full subtype list and common equipment facets remain typed. Additional subtype-specific fields not yet modelled remain in AdditionalProperties.
- Unmodelled fields are preserved via JsonExtensionData. Known model fields are eagerly deserialized only once, as the record is read.
- Unit strings (e.g. 68 g or 15 m) are captured as CddaScalar objects; physical-unit arithmetic and normalization are not implemented.
- Files are expected to contain arrays of definitions. Directory enumeration processes files in path order and returns source paths.
- Tests use reduced, representative source records from the fork. **The entire data tree has not yet been exhaustively validated.**

## License

MIT; see LICENSE.
