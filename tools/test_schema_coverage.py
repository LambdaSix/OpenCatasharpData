import tempfile
import unittest
from pathlib import Path

from schema_coverage import dto_fields, inventory, remove_jsonc_extras, report


class SchemaCoverageTests(unittest.TestCase):
    def test_strip_comments_and_trailing_commas_but_not_strings(self):
        text = '[{"type":"ITEM", "description":"https://example/a,b", /* x */ "id":"a",}, // y\n]'
        parsed = remove_jsonc_extras(text)
        self.assertIn('"https://example/a,b"', parsed)
        import json
        self.assertEqual(json.loads(parsed)[0]["id"], "a")

    def test_inventories_mixed_subtypes_without_double_count(self):
        with tempfile.TemporaryDirectory() as tmp:
            directory = Path(tmp)
            data = directory / "data"
            models = directory / "models"
            data.mkdir()
            models.mkdir()
            (models / "DTO.cs").write_text(
                'public abstract class CddaDefinition { [JsonPropertyName("type")] public string Type; }'
                'public class ItemDefinition { [JsonPropertyName("id")] public string Id; }'
                'public sealed class ArmorData { [JsonPropertyName("armor")] public object Armor; }',
                encoding="utf-8",
            )
            (data / "a.json").write_text(
                '[{"type":"ITEM","id":"a","subtypes":["ARMOR","MAGAZINE"],'
                '"armor":[{"covers":["torso"]}],"new_field":1},'
                '{"type":"ITEM","id":"b","subtypes":["TOOL"],"other":true},'
                '{"type":"other","id":"ignored"}]',
                encoding="utf-8",
            )
            result = inventory(data, dto_fields(models))
            self.assertEqual(result["items"], 2)
            self.assertEqual(result["subtype_counts"]["ARMOR"], 1)
            self.assertEqual(result["subtype_counts"]["MAGAZINE"], 1)
            self.assertEqual(result["routing_counts"]["ARMOR+MAGAZINE"], 1)
            self.assertEqual(result["unmodelled"]["new_field"], 1)
            self.assertIn("Nested field gaps", report(result, "forksha", "modelsha"))


if __name__ == "__main__":
    unittest.main()
