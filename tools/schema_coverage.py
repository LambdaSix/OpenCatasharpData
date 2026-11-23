#!/usr/bin/env python3
"""Inventory actual CDDA ITEM JSON fields against eager DTO properties.

Run on a pinned local checkout of data/json/items. This is field-name/shape
coverage, NOT a proof of successful C# deserialization, inheritance resolution
or equivalence of CDDA game semantics.
"""
import argparse
from collections import Counter, defaultdict
import json
from pathlib import Path
import re


SUBTYPES = ("ARMOR", "COMESTIBLE", "TOOL", "GUN", "AMMO",
            "MAGAZINE", "BOOK", "GUNMOD", "BIONIC_ITEM", "SEED",
            "BREWABLE", "COMPOSTABLE", "MILLING", "ENGINE", "WHEEL",
            "PET_ARMOR", "TOOLMOD", "ARTIFACT")
CLASS_FOR_SUBTYPE = {
    "ARMOR": "ArmorData",
    "COMESTIBLE": "ComestibleData",
    "TOOL": "ToolData",
    "GUN": "GunData",
    "AMMO": "AmmoData",
    "MAGAZINE": "MagazineData",
    "BOOK": "BookData",
    "GUNMOD": "GunModData",
    "BIONIC_ITEM": "BionicItemData",
    "SEED": "SeedData",
    "BREWABLE": "BrewableData",
    "COMPOSTABLE": "CompostableData",
    "MILLING": "MillingData",
    "ENGINE": "EngineData",
    "WHEEL": "WheelData",
    "PET_ARMOR": "PetArmorData",
    "TOOLMOD": "ToolModData",
    "ARTIFACT": "ArtifactData",
}
NESTED_MODEL = {
    "armor": "CddaArmorSection",
    "contamination": "CddaContamination",
    "rot_spawn": "CddaRotSpawn",
    "addiction_type": "CddaAddiction",
    "consumption_effect_on_conditions": "CddaEffectReference",
    "pocket_data": "CddaPocket",
    "material": "CddaMaterialPart",
    "variants": "CddaItemVariant",
    "use_action": "CddaUseAction",
    "melee_damage": "CddaMeleeDamage",
    "to_hit": "CddaToHit",
    "damage": "CddaDamage",
    "shot_damage": "CddaDamage",
    "ranged_damage": "CddaDamage",
    "modes": "CddaGunMode",
    "passive_effects": "CddaArtifactPassiveEffect",
}
INTERESTING = (
    "melee_damage", "to_hit", "variants", "use_action", "modes", "damage",
    "shot_damage", "ranged_damage", "magazines", "valid_mod_locations",
    "ammo", "pocket_data", "armor", "material", "qualities",
)


def remove_jsonc_extras(source: str) -> str:
    """Remove JS-style comments and trailing commas outside JSON strings."""
    result = []
    i = 0
    quoted = False
    while i < len(source):
        char = source[i]
        if quoted:
            result.append(char)
            if char == "\\" and i + 1 < len(source):
                i += 1
                result.append(source[i])
            elif char == '"':
                quoted = False
            i += 1
            continue
        if char == '"':
            quoted = True
            result.append(char)
            i += 1
        elif source.startswith("//", i):
            i += 2
            while i < len(source) and source[i] not in "\r\n":
                i += 1
        elif source.startswith("/*", i):
            i += 2
            while i + 1 < len(source) and source[i:i + 2] != "*/":
                i += 1
            if i + 1 >= len(source):
                raise ValueError("Unterminated block comment")
            i += 2
        else:
            result.append(char)
            i += 1

    text = "".join(result)
    # JSON strings are scanned again to avoid mangling literal commas.
    output = []
    quoted = False
    i = 0
    while i < len(text):
        char = text[i]
        if quoted:
            output.append(char)
            if char == "\\" and i + 1 < len(text):
                i += 1
                output.append(text[i])
            elif char == '"':
                quoted = False
        elif char == '"':
            quoted = True
            output.append(char)
        elif char == ",":
            next_index = i + 1
            while next_index < len(text) and text[next_index].isspace():
                next_index += 1
            if next_index < len(text) and text[next_index] in "}]":
                i += 1
                continue
            output.append(char)
        else:
            output.append(char)
        i += 1
    return "".join(output)


def dto_fields(model_dir: Path):
    """Identify declared eager fields in C# classes, using attribute annotations."""
    models = {}
    class_start = re.compile(r"\b(?:public|internal)\s+(?:(?:sealed|abstract|partial)\s+)*class\s+(\w+)")
    field_attribute = re.compile(r'\[JsonPropertyName\("([^"]+)"\)\]')
    for file in sorted(model_dir.glob("*.cs")):
        contents = file.read_text(encoding="utf-8")
        found = list(class_start.finditer(contents))
        for index, match in enumerate(found):
            end = found[index + 1].start() if index + 1 < len(found) else len(contents)
            models[match.group(1)] = set(field_attribute.findall(contents[match.end():end]))
    for required in ("CddaDefinition", "ItemDefinition"):
        if required not in models:
            raise ValueError(f"Missing DTO {required} in {model_dir}")
    return models


def json_shape(value):
    if isinstance(value, dict):
        return "object"
    if isinstance(value, list):
        return "array"
    if value is None:
        return "null"
    if isinstance(value, bool):
        return "boolean"
    if isinstance(value, (int, float)):
        return "number"
    return "string"


def inventory(data_dir: Path, models: dict[str, set[str]]):
    if not data_dir.is_dir():
        raise ValueError(f"Missing item JSON directory: {data_dir}")
    stats = {
        "files": 0,
        "records": 0,
        "items": 0,
        "other_type_counts": Counter(),
        "subtype_counts": Counter(),
        "routing_counts": Counter(),
        "fields": Counter(),
        "unmodelled": Counter(),
        "shapes": defaultdict(Counter),
        "nested_fields": Counter(),
        "nested_missing": Counter(),
        "examples": {},
        "total_fields": 0,
        "mapped_fields": 0,
    }
    base = models["CddaDefinition"] | models["ItemDefinition"]
    paths = sorted(data_dir.rglob("*.json"))
    if not paths:
        raise ValueError(f"No JSON files below {data_dir}")
    for path in paths:
        try:
            content = remove_jsonc_extras(path.read_text(encoding="utf-8-sig"))
            data = json.loads(content)
        except (ValueError, OSError) as exc:
            raise ValueError(f"Unable to parse {path}: {exc}") from exc
        if not isinstance(data, list):
            raise ValueError(f"{path}: expected a JSON array")
        stats["files"] += 1
        for record in data:
            if not isinstance(record, dict):
                raise ValueError(f"{path}: expected each record to be an object")
            stats["records"] += 1
            if str(record.get("type", "")).upper() != "ITEM":
                stats["other_type_counts"][str(record.get("type"))] += 1
                continue
            stats["items"] += 1
            declared = record.get("subtypes", [])
            if not isinstance(declared, list):
                raise ValueError(f"{path}: item subtypes should be an array")
            for subtype in set(x for x in declared if isinstance(x, str)):
                stats["subtype_counts"][subtype.upper()] += 1
            # Multiple declarations can coexist; the parser now loads every
            # matching subtype into independent typed components.
            selected = list(dict.fromkeys(
                x.upper() for x in declared
                if isinstance(x, str) and x.upper() in SUBTYPES
            ))
            stats["routing_counts"]["+".join(selected) if selected else "GENERIC"] += 1
            known = set(base)
            for subtype in selected:
                known.update(models.get(CLASS_FOR_SUBTYPE[subtype], set()))
            source_id = record.get("id") or record.get("abstract") or "anonymous"
            source = f"{path.relative_to(data_dir)}:{source_id}"
            for name, value in record.items():
                # CDDA JSON comments are often encoded as //, //1 and so on.
                if name.startswith("//"):
                    continue
                stats["total_fields"] += 1
                stats["fields"][name] += 1
                stats["shapes"][name][json_shape(value)] += 1
                stats["examples"].setdefault(name, source)
                if name in known:
                    stats["mapped_fields"] += 1
                else:
                    stats["unmodelled"][name] += 1
                # Nested observations are diagnostic; only modelled object
                # properties count towards the top-level coverage percentage.
                if name not in NESTED_MODEL:
                    continue
                objects = value if isinstance(value, list) else [value]
                nested_known = models.get(NESTED_MODEL[name], set())
                for obj in objects:
                    if not isinstance(obj, dict):
                        continue
                    for nested_name in obj:
                        if nested_name.startswith("//"):
                            continue
                        label = f"{name}.{nested_name}"
                        stats["nested_fields"][label] += 1
                        if nested_name not in nested_known:
                            stats["nested_missing"][label] += 1
    if stats["items"] == 0:
        raise ValueError("No ITEM records found in supplied dataset")
    return stats


def report(stats, source_revision: str, model_revision: str):
    quote = chr(96)
    percentage = 100 * stats["mapped_fields"] / stats["total_fields"]
    lines = [
        "# CDDA Item Schema Coverage",
        "",
        f"Source: {quote}LambdaSix/Cataclysm-DDA{quote} at {quote}{source_revision}{quote}.",
        f"DTO reference: {quote}OpenCatasharpData{quote} at {quote}{model_revision}{quote}.",
        "Scope: core " + quote + "data/json/items/**/*.json" + quote +
        " (not mods). Counts describe authored source records, not resolved game entities.",
        "",
        "## Inventory",
        "",
        f"- JSON files: **{stats['files']:,}**",
        f"- All records in those files: **{stats['records']:,}**",
        f"- Records with " + quote + "type: ITEM" + quote + f": **{stats['items']:,}**",
        f"- Unique top-level ITEM field names: **{len(stats['fields']):,}**",
        f"- Authored top-level field occurrences with matching DTO property: "
        f"**{stats['mapped_fields']:,}/{stats['total_fields']:,} ({percentage:.1f}%)**",
        "",
        "This is *declared-property coverage*, not a runtime deserialization pass or deep-schema coverage. "
        "Counts exclude comment-like keys beginning with //; unknown fields remain preserved by JsonExtensionData.",
        "",
        "## Subtype membership (overlapping)",
        "",
        "| Declared subtype | ITEM records | Built-in handler |",
        "|---|---:|---|",
    ]
    for subtype, count in stats["subtype_counts"].most_common():
        lines.append(f"| {quote}{subtype}{quote} | {count:,} | " +
                     ("yes" if subtype in SUBTYPES else "no") + " |")
    lines += [
        "", "## Composed subtype signatures (exclusive)", "",
        "| Selected type | Records |", "|---|---:|",
    ]
    for name, count in stats["routing_counts"].most_common():
        lines.append(f"| {quote}{name}{quote} | {count:,} |")
    lines += [
        "", "## Remaining unmodelled item fields (complete inventory)", "",
        "| Field | ITEM records | Observed shapes | Example record |",
        "|---|---:|---|---|",
    ]
    for name, count in stats["unmodelled"].most_common():
        shapes = ", ".join(f"{kind} ({amount})" for kind, amount in stats["shapes"][name].most_common())
        lines.append(
            f"| {quote}{name}{quote} | {count:,} | {shapes} | "
            f"{quote}{stats['examples'][name]}{quote} |"
        )
    lines += ["", "## Selected complex fields (including already modelled)", "",
              "| Field | Records | Observed shapes | Typed top-level property |",
              "|---|---:|---|---|"]
    for name in INTERESTING:
        count = stats["fields"][name]
        shapes = ", ".join(f"{kind} ({v})" for kind, v in stats["shapes"][name].most_common())
        lines.append(f"| {quote}{name}{quote} | {count:,} | {shapes or '-'} | " +
                     ("yes" if name not in stats["unmodelled"] and count else "partial/no") + " |")
    lines += [
        "", "## Nested field gaps", "",
        "These are descriptive counts for nested objects (e.g. armor sections and pocket_data). "
        "They are not included in the top-level percentage.",
        "",
        "| Nested path | Occurrences without matching nested DTO property |",
        "|---|---:|",
    ]
    for name, count in stats["nested_missing"].most_common():
        lines.append(f"| {quote}{name}{quote} | {count:,} |")
    if stats["other_type_counts"]:
        lines += ["", "## Other discriminators in the item directory", "",
                  "| Type | Records |", "|---|---:|"]
        for name, count in stats["other_type_counts"].most_common(20):
            lines.append(f"| {quote}{name}{quote} | {count:,} |")
    lines += [
        "", "## Interpretation and limitations", "",
        "- Unknown source fields are preserved, but are not typed until a DTO/converter is introduced.",
        "- Declared subtypes count for every membership; the parser attaches all supported subtype components.",
        "- A C# property name match is not proof that every observed JSON value shape deserializes.",
        "- Inheritance directives and patch operations are inventoried but not applied.",
        "- Re-run against the same fork SHA before and after DTO changes to obtain comparable coverage.",
        "",
    ]
    return "\n".join(lines)



def require_coverage(stats, minimum_percent: float):
    """Assert a minimum coverage percentage for the pinned source inventory."""
    if not 0 <= minimum_percent <= 100:
        raise ValueError("Minimum coverage must be between 0 and 100.")
    total = stats["total_fields"]
    percent = 100 * stats["mapped_fields"] / total if total else 0
    if percent < minimum_percent:
        fields = ", ".join(
            f"{name} ({count})" for name, count in stats["unmodelled"].most_common(12)
        )
        raise ValueError(
            f"Top-level item DTO coverage {percent:.2f}% is below the "
            f"required {minimum_percent:.2f}%. Unmapped: {fields or 'none'}"
        )

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--data-dir", type=Path, required=True)
    parser.add_argument("--model-dir", type=Path, required=True)
    parser.add_argument("--source-revision", default="local")
    parser.add_argument("--model-revision", default="local")
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--min-coverage", type=float, default=None)
    args = parser.parse_args()
    result = inventory(args.data_dir, dto_fields(args.model_dir))
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(report(result, args.source_revision, args.model_revision), encoding="utf-8")
    print(f"Scanned {result['files']} JSON files and {result['items']} ITEM records.")
    print(f"Typed field occurrences: {result['mapped_fields']}/{result['total_fields']}")
    print(f"Wrote {args.output}")
    if args.min_coverage is not None:
        try:
            require_coverage(result, args.min_coverage)
        except ValueError as error:
            parser.exit(1, f"Coverage check failed: {error}\n")
        print(f"Coverage gate met: >= {args.min_coverage:g}%")



if __name__ == "__main__":
    main()
