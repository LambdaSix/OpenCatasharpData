using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace OpenCatasharpData.Resolution;

/// <summary>
/// Field-shape-specific operations for first-pass item source inheritance.
/// All mutations operate only on deep-cloned JsonNode instances.
/// Unsupported operations fail with source/path context instead of silently
/// producing incorrect effective game data.
/// </summary>
internal static class CddaItemPatch
{
    private static readonly Regex QuantityPattern = new(
        @"^\s*([+-]?(?:\d+(?:\.\d+)?|\.\d+))\s+([^\s]+)\s*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    internal static void ApplyNumeric(
        JsonObject inherited, JsonNode? patch, bool proportional, CddaSourceRecord source)
    {
        if (patch is null) return;
        if (patch is not JsonObject map)
            throw Error(source, "", "relative/proportional must contain a JSON object.");

        foreach (var property in map)
        {
            if (!inherited.TryGetPropertyValue(property.Key, out var oldValue))
                throw Error(source, property.Key, "Numeric patch refers to an absent inherited field.");

            inherited[property.Key] = TransformNumeric(
                oldValue, property.Value, proportional, source, property.Key);
        }
    }

    private static JsonNode? TransformNumeric(
        JsonNode? original, JsonNode? patch, bool proportional,
        CddaSourceRecord source, string path)
    {
        if (original is JsonObject obj && patch is JsonObject changes)
        {
            var result = (JsonObject)obj.DeepClone();
            foreach (var property in changes)
            {
                if (!result.TryGetPropertyValue(property.Key, out var oldValue))
                    throw Error(source, $"{path}.{property.Key}",
                        "Numeric patch refers to an absent inherited field.");

                result[property.Key] = TransformNumeric(
                    oldValue, property.Value, proportional, source, $"{path}.{property.Key}");
            }
            return result;
        }

        if (original is JsonArray array && patch is JsonArray patchArray)
        {
            var result = (JsonArray)array.DeepClone();
            foreach (var (entry, index) in patchArray.Select((entry, index) => (entry, index)))
            {
                var matchedIndex = FindMatchingIndex(result, entry);
                if (matchedIndex < 0)
                {
                    if (patchArray.Count != array.Count)
                        throw Error(source, path, "Cannot match a numeric array patch to inherited entries.");
                    matchedIndex = index;
                }

                result[matchedIndex] = TransformNumeric(
                    result[matchedIndex], entry, proportional, source, $"{path}[{matchedIndex}]");
            }
            return result;
        }

        if (patch is null || original is null)
            throw Error(source, path, "A null value cannot participate in a numeric patch.");

        // Discriminator hints such as {"damage_type":"bullet"} select the
        // inherited damage type. They are not mathematical operands.
        if (IsString(patch))
        {
            if (JsonNode.DeepEquals(original, patch))
                return original.DeepClone();
            throw Error(source, path, "String discriminator does not match the inherited value.");
        }

        if (!TryDecimal(patch, out var operand))
            throw Error(source, path, "Expected a numeric patch factor/offset.");

        if (TryDecimal(original, out var previous))
            return JsonValue.Create(proportional ? previous * operand : previous + operand);

        if (IsString(original) && proportional)
        {
            var text = JsonSerializer.Deserialize<string>(original.ToJsonString())!;
            var match = QuantityPattern.Match(text);
            if (!match.Success ||
                !decimal.TryParse(match.Groups[1].Value, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var amount))
                throw Error(source, path, $"Unsupported compound quantity '{text}'.");

            var modified = amount * operand;
            return JsonValue.Create(
                modified.ToString("G29", CultureInfo.InvariantCulture) + " " + match.Groups[2].Value);
        }

        throw Error(source, path, "Numeric patch cannot be applied to this JSON value shape.");
    }

    private static int FindMatchingIndex(JsonArray target, JsonNode? patch)
    {
        if (patch is not JsonObject selector) return -1;
        foreach (var name in new[] { "damage_type", "id", "type" })
        {
            if (!selector.TryGetPropertyValue(name, out var discriminator)) continue;
            for (var index = 0; index < target.Count; index++)
                if (target[index] is JsonObject entry &&
                    entry.TryGetPropertyValue(name, out var candidate) &&
                    JsonNode.DeepEquals(candidate, discriminator))
                    return index;
        }
        return -1;
    }

    internal static void ApplyContainers(
        JsonObject destination, JsonNode? patch, bool delete, CddaSourceRecord source)
    {
        if (patch is null) return;
        if (patch is not JsonObject fields)
            throw Error(source, "", "extend/delete must contain a JSON object.");

        ApplyMap(destination, fields, delete, source, "");
    }

    private static void ApplyMap(
        JsonObject target, JsonObject patch, bool delete, CddaSourceRecord source, string path)
    {
        foreach (var property in patch)
        {
            var key = property.Key;
            var fieldPath = path.Length == 0 ? key : $"{path}.{key}";
            if (!target.TryGetPropertyValue(key, out var previous))
            {
                if (!delete) target[key] = property.Value?.DeepClone();
                continue;
            }

            if (previous is JsonArray targetArray && property.Value is JsonArray incoming)
            {
                if (delete)
                {
                    for (var index = targetArray.Count - 1; index >= 0; index--)
                        if (incoming.Any(entry => JsonNode.DeepEquals(entry, targetArray[index])))
                            targetArray.RemoveAt(index);
                }
                else
                {
                    foreach (var entry in incoming)
                        targetArray.Add(entry?.DeepClone());
                }
            }
            else if (previous is JsonObject targetObject && property.Value is JsonObject update)
            {
                ApplyMap(targetObject, update, delete, source, fieldPath);
            }
            else if (delete && JsonNode.DeepEquals(previous, property.Value))
            {
                target.Remove(key);
            }
            else
            {
                throw Error(source, fieldPath,
                    $"Unsupported {(delete ? "delete" : "extend")} patch field shape.");
            }
        }
    }

    private static bool TryDecimal(JsonNode node, out decimal number) =>
        decimal.TryParse(node.ToJsonString(), NumberStyles.Float,
            CultureInfo.InvariantCulture, out number);

    private static bool IsString(JsonNode node) =>
        node is JsonValue && node.ToJsonString().StartsWith("\"", StringComparison.Ordinal);

    private static CddaResolutionException Error(
        CddaSourceRecord source, string path, string message) =>
        new(source, $"Patch at '{(path.Length == 0 ? "<root>" : path)}': {message}");
}
