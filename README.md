# OpenCatasharpData

A small .NET 10 class library for reading **Cataclysm: Dark Days Ahead** (CDDA) data JSON using **System.Text.Json**.

The initial goal is to read definitions from [LambdaSix/Cataclysm-DDA](https://github.com/LambdaSix/Cataclysm-DDA), expose useful typed DTOs, and preserve fields that have not been modeled yet.

## Getting started

Requires the .NET 10 SDK. The library has no third-party runtime dependencies.

~~~bash
dotnet build OpenCatasharpData.slnx
dotnet run --project tests/OpenCatasharpData.SmokeTests
~~~

Reference the library project from an application:

~~~xml
<ProjectReference Include="../OpenCatasharpData/src/OpenCatasharpData/OpenCatasharpData.csproj" />
~~~

Read a file:

~~~csharp
using OpenCatasharpData;
using OpenCatasharpData.Models;

var definitions = await CddaJsonLoader.LoadFileAsync(
    "/path/to/Cataclysm-DDA/data/json/skills.json");

foreach (var skill in definitions.OfType<SkillDefinition>())
{
    Console.WriteLine($"{skill.IdString}: {skill.Name?.Singular}");
}
~~~

Read a directory recursively (one JSON file at a time):

~~~csharp
await foreach (var entry in CddaJsonLoader.LoadDirectoryAsync(
    "/path/to/Cataclysm-DDA/data/json"))
{
    Console.WriteLine($"{entry.Definition.Type}: {entry.Definition.IdString} ({entry.FilePath})");
}
~~~

## Current typed definitions

- SkillDefinition (type: skill), including level descriptions
- MaterialDefinition (type: material)
- ItemCategoryDefinition (type: ITEM_CATEGORY)
- AmmunitionTypeDefinition (type: ammunition_type)
- ItemDefinition for common item type discriminators (ITEM, GENERIC, ARMOR, TOOL, GUN, AMMO, COMESTIBLE, BOOK, GUNMOD, MAGAZINE, BATTERY)
- RecipeDefinition (type: recipe)
- MonsterDefinition (type: MONSTER)
- UnknownDefinition for anything else

All definitions inherit common fields from CddaDefinition. The **type** discriminator selects a DTO, regardless of source filename. Unmodeled properties are retained in AdditionalProperties through JsonExtensionData. Fields with multiple shapes can be kept as JsonElement rather than guessed at.

The CDDA name/description format may be a string or an object (for example, {"str":"social"} or {"str_sp":"software"}). CddaText handles both forms without losing the original JSON.

## Scope and limitations

This project loads **source definitions**, not the fully resolved in-game content database. It does **not** yet apply copy-from inheritance, abstract expansion, extend, delete, relative, or proportional patches. Those source properties are preserved for later processing.

Input files are expected to be JSON arrays of typed objects, as described in the [CDDA JSON documentation](https://docs.cataclysmdda.org/JSON/JSON_INFO.html). The loader supports trailing commas and JSON comments; CDDA's comment-like properties such as "//" are retained as ordinary JSON fields.

A sensible next step is to add a resolver and validation tests using representative fixtures from the fork, then expand DTO coverage by category.

## License

MIT; see LICENSE.
