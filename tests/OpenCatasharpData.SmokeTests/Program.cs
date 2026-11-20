using System.Text;
using System.Text.Json;
using OpenCatasharpData;
using OpenCatasharpData.Models;

const string sample = """
[
  {
    "type": "skill",
    "id": "speech",
    "name": { "str": "social" },
    "description": "Persuasion and bartering.",
    "sort_rank": 26000,
    "level_descriptions_theory": [
      { "level": 0, "description": "Newcomer." }
    ]
  },
  {
    "type": "material",
    "abstract": "polymer",
    "name": "Polymer",
    "density": 1.25,
    "copy-from": "base_material",
    "relative": { "chip_resist": 2 }
  },
  {
    "type": "ITEM_CATEGORY",
    "id": "software",
    "name_header": { "str": "Software" },
    "name_noun": { "str_sp": "software" }
  },
  {
    "type": "ITEM",
    "id": "fire",
    "name": { "str_sp": "nearby fire" },
    "material": ["wood", "steel"],
    "custom_attribute": { "enabled": true }
  },
  {
    "type": "unmodeled_type",
    "id": ["first", "second"],
    "special": 42
  }
]
""";

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(sample));
var definitions = await CddaJsonLoader.LoadAsync(stream);

Check(definitions.Count == 5, "Definition count");
Check(definitions[0] is SkillDefinition, "Skill discriminator");
var skill = (SkillDefinition)definitions[0];
Check(skill.IdString == "speech", "String ID");
Check(skill.Name?.Singular == "social", "Object translation");
Check(skill.Description?.Singular == "Persuasion and bartering.", "String translation");
Check(skill.LevelDescriptionsTheory?[0].Level == 0, "Nested typed DTO");

var material = definitions[1] as MaterialDefinition;
Check(material is not null && material.Name?.Singular == "Polymer", "Material DTO");
Check(material?.CopyFrom == "base_material", "Inheritance metadata");
Check(material?.Relative?.ValueKind == JsonValueKind.Object, "Relative patch preservation");

var category = definitions[2] as ItemCategoryDefinition;
Check(category?.NameNoun?.Singular == "software", "str_sp translation");

var item = definitions[3] as ItemDefinition;
Check(item?.AdditionalProperties?["custom_attribute"].GetProperty("enabled").GetBoolean() == true,
    "Unknown field preservation");
Check(item?.Material?.ValueKind == JsonValueKind.Array, "Variable shape preservation");

var unknown = definitions[4] as UnknownDefinition;
Check(unknown?.IdString is null, "Non-string IDs do not break loading");
Check(unknown?.AdditionalProperties?["special"].GetInt32() == 42,
    "Unknown definition preservation");

var directory = Path.Combine(Path.GetTempPath(), "OpenCatasharpData-" + Guid.NewGuid());
Directory.CreateDirectory(Path.Combine(directory, "nested"));
try
{
    await File.WriteAllTextAsync(Path.Combine(directory, "nested", "sample.json"), sample);
    var count = 0;
    await foreach (var entry in CddaJsonLoader.LoadDirectoryAsync(directory))
    {
        Check(entry.FilePath.EndsWith("sample.json", StringComparison.Ordinal),
            "Source filename");
        count++;
    }
    Check(count == 5, "Recursive directory loader");
}
finally
{
    Directory.Delete(directory, recursive: true);
}

Console.WriteLine("OpenCatasharpData smoke tests passed.");

static void Check(bool success, string label)
{
    if (!success)
    {
        throw new InvalidOperationException($"FAILED: {label}");
    }
}
