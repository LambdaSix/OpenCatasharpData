using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>Typed components of an item's melee damage. Unknown damage types are preserved.</summary>
public sealed class CddaMeleeDamage
{
    [JsonPropertyName("bash")]
    public decimal? Bash { get; set; }

    [JsonPropertyName("cut")]
    public decimal? Cut { get; set; }

    [JsonPropertyName("stab")]
    public decimal? Stab { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Either a legacy numeric to-hit bonus or its four-part modern description.</summary>
[JsonConverter(typeof(CddaToHitJsonConverter))]
public sealed class CddaToHit
{
    public decimal? LegacyBonus { get; set; }
    public string? Grip { get; set; }
    public string? Length { get; set; }
    public string? Surface { get; set; }
    public string? Balance { get; set; }
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

public sealed class CddaToHitJsonConverter : JsonConverter<CddaToHit>
{
    public override CddaToHit? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var element = doc.RootElement;
        if (element.ValueKind == JsonValueKind.Null) return null;
        if (element.ValueKind == JsonValueKind.Number)
            return new CddaToHit { LegacyBonus = element.GetDecimal() };
        if (element.ValueKind != JsonValueKind.Object)
            throw new JsonException("Expected to_hit as a number or object.");

        var value = new CddaToHit();
        foreach (var field in element.EnumerateObject())
        {
            switch (field.Name)
            {
                case "grip": value.Grip = field.Value.GetString(); break;
                case "length": value.Length = field.Value.GetString(); break;
                case "surface": value.Surface = field.Value.GetString(); break;
                case "balance": value.Balance = field.Value.GetString(); break;
                default:
                    (value.AdditionalProperties ??= new())[field.Name] = field.Value.Clone();
                    break;
            }
        }
        return value;
    }

    public override void Write(Utf8JsonWriter writer, CddaToHit value, JsonSerializerOptions options)
    {
        if (value.LegacyBonus is decimal legacy)
        {
            writer.WriteNumberValue(legacy);
            return;
        }
        writer.WriteStartObject();
        if (value.Grip is not null) writer.WriteString("grip", value.Grip);
        if (value.Length is not null) writer.WriteString("length", value.Length);
        if (value.Surface is not null) writer.WriteString("surface", value.Surface);
        if (value.Balance is not null) writer.WriteString("balance", value.Balance);
        if (value.AdditionalProperties is not null)
            foreach (var field in value.AdditionalProperties)
            {
                writer.WritePropertyName(field.Key);
                field.Value.WriteTo(writer);
            }
        writer.WriteEndObject();
    }
}

/// <summary>One cosmetic variant, eagerly materialised from the source JSON.</summary>
public sealed class CddaItemVariant
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public CddaText? Name { get; set; }

    [JsonPropertyName("description")]
    public CddaText? Description { get; set; }

    [JsonPropertyName("weight")]
    public decimal? Weight { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("ascii_picture")]
    public string? AsciiPicture { get; set; }

    [JsonPropertyName("append")]
    public bool? Append { get; set; }

    [JsonPropertyName("expand_snippets")]
    public bool? ExpandSnippets { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A typed action descriptor. Action-specific fields remain extensible.</summary>
public sealed class CddaUseAction
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("menu_text")]
    public CddaText? MenuText { get; set; }

    [JsonPropertyName("msg")]
    public CddaText? Message { get; set; }

    [JsonPropertyName("moves")]
    public decimal? Moves { get; set; }

    [JsonPropertyName("sound_volume")]
    public int? SoundVolume { get; set; }

    [JsonPropertyName("cable_length")]
    public int? CableLength { get; set; }

    [JsonPropertyName("charge_rate")]
    public CddaScalar? ChargeRate { get; set; }

    /// <summary>Numeric parameter of tuple-style actions such as ["CROWBAR", 0].</summary>
    [JsonIgnore]
    public decimal? NumericArgument { get; internal set; }

    [JsonIgnore]
    public bool WasTuple { get; internal set; }

    [JsonIgnore]
    public bool WasString { get; internal set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>
/// Accepts a single action string, a single action object or a mixed array.
/// All known action fields are converted before returning the DTO.
/// </summary>
public sealed class CddaUseActionsJsonConverter : JsonConverter<List<CddaUseAction>>
{
    public override List<CddaUseAction>? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Null) return null;

        var actions = new List<CddaUseAction>();
        if (root.ValueKind == JsonValueKind.Array)
        {
            // Sometimes a tuple itself is the whole use_action value.
            if (root.GetArrayLength() == 2 &&
                root[0].ValueKind == JsonValueKind.String &&
                root[1].ValueKind == JsonValueKind.Number)
                actions.Add(ReadAction(root, options));
            else
                foreach (var element in root.EnumerateArray())
                    actions.Add(ReadAction(element, options));
        }
        else
            actions.Add(ReadAction(root, options));

        return actions;
    }

    private static CddaUseAction ReadAction(JsonElement item, JsonSerializerOptions options) =>
        item.ValueKind switch
        {
            JsonValueKind.String =>
                new CddaUseAction { Type = item.GetString(), WasString = true },
            JsonValueKind.Array when item.GetArrayLength() == 2 =>
                new CddaUseAction
                {
                    Type = item[0].GetString(),
                    NumericArgument = item[1].GetDecimal(),
                    WasTuple = true
                },
            JsonValueKind.Object =>
                item.Deserialize<CddaUseAction>(options)
                ?? throw new JsonException("Invalid use action."),
            _ => throw new JsonException("Expected a use_action string or object.")
        };

    public override void Write(
        Utf8JsonWriter writer, List<CddaUseAction> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            if (item.WasString) writer.WriteStringValue(item.Type);
            else if (item.WasTuple)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(item.Type);
                writer.WriteNumberValue(item.NumericArgument ?? 0);
                writer.WriteEndArray();
            }
            else JsonSerializer.Serialize(writer, item, options);
        }
        writer.WriteEndArray();
    }
}

public sealed class CddaBarrelDamage
{
    [JsonPropertyName("barrel_length")]
    public CddaLength? BarrelLength { get; set; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Projectile damage description, with a typed barrel-damage curve.</summary>
public sealed class CddaDamage
{
    // Ranged damage may use the legacy numeric shorthand (e.g. "ranged_damage": 0).
    [JsonIgnore]
    public bool WasNumber { get; internal set; }

    [JsonIgnore]
    public bool WasSingleObject { get; internal set; }

    [JsonPropertyName("damage_type")]
    public string? DamageType { get; set; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("armor_penetration")]
    public decimal? ArmorPenetration { get; set; }

    [JsonPropertyName("damage_multiplier")]
    public decimal? DamageMultiplier { get; set; }

    [JsonPropertyName("barrels")]
    public List<CddaBarrelDamage>? Barrels { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Reads ranged_damage as either one damage object or an array of them.</summary>
public sealed class CddaDamageListJsonConverter : JsonConverter<List<CddaDamage>>
{
    public override List<CddaDamage>? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Null) return null;
        var result = new List<CddaDamage>();
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in root.EnumerateArray())
                result.Add(entry.Deserialize<CddaDamage>(options)
                    ?? throw new JsonException("Invalid ranged damage entry."));
        }
        else if (root.ValueKind == JsonValueKind.Object)
        {
            var value = root.Deserialize<CddaDamage>(options)
                ?? throw new JsonException("Invalid ranged damage.");
            value.WasSingleObject = true;
            result.Add(value);
        }
        else if (root.ValueKind == JsonValueKind.Number)
            result.Add(new CddaDamage { Amount = root.GetDecimal(), WasNumber = true });
        else throw new JsonException("Expected ranged_damage as a number, object or array.");
        return result;
    }

    public override void Write(Utf8JsonWriter writer, List<CddaDamage> value, JsonSerializerOptions options)
    {
        if (value.Count == 1 && value[0].WasNumber && value[0].Amount is { } amount)
            writer.WriteNumberValue(amount);
        else if (value.Count == 1 && value[0].WasSingleObject)
            JsonSerializer.Serialize(writer, value[0], options);
        else
            JsonSerializer.Serialize(writer, value, options);
    }
}

/// <summary>One tuple of gun mode ID, display label, shots, and optional flag.</summary>
[JsonConverter(typeof(CddaGunModeJsonConverter))]
public sealed class CddaGunMode
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Shots { get; set; }
    public string? Flag { get; set; }
    public List<string>? Flags { get; set; }
    public List<JsonElement>? ExtraValues { get; set; }
}

public sealed class CddaGunModeJsonConverter : JsonConverter<CddaGunMode>
{
    public override CddaGunMode? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var array = doc.RootElement;
        if (array.ValueKind == JsonValueKind.Null) return null;
        if (array.ValueKind != JsonValueKind.Array || array.GetArrayLength() < 3)
            throw new JsonException("Gun mode must be [id, label, shots, optional flag].");

        var result = new CddaGunMode
        {
            Id = array[0].GetString()!,
            Label = array[1].GetString()!,
            Shots = array[2].GetInt32(),
            Flag = array.GetArrayLength() > 3 && array[3].ValueKind == JsonValueKind.String
                ? array[3].GetString() : null,
            Flags = array.GetArrayLength() > 3 && array[3].ValueKind == JsonValueKind.Array
                ? array[3].EnumerateArray().Select(flag =>
                    flag.ValueKind == JsonValueKind.String
                        ? flag.GetString()!
                        : throw new JsonException("Gun mode flags must be strings.")).ToList()
                : null
        };
        if (array.GetArrayLength() > 4)
            result.ExtraValues = array.EnumerateArray().Skip(4).Select(e => e.Clone()).ToList();
        return result;
    }

    public override void Write(Utf8JsonWriter writer, CddaGunMode value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.Id);
        writer.WriteStringValue(value.Label);
        writer.WriteNumberValue(value.Shots);
        if (value.Flags is not null) JsonSerializer.Serialize(writer, value.Flags, options);
        else if (value.Flag is not null) writer.WriteStringValue(value.Flag);
        if (value.ExtraValues is not null)
            foreach (var item in value.ExtraValues) item.WriteTo(writer);
        writer.WriteEndArray();
    }
}

public sealed class CddaConditionalName
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("condition")]
    public string? Condition { get; set; }

    [JsonPropertyName("name")]
    public CddaText? Name { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

public sealed class CddaFault
{
    [JsonPropertyName("weight")]
    public decimal? Weight { get; set; }

    [JsonPropertyName("weight_override")]
    public decimal? WeightOverride { get; set; }

    [JsonPropertyName("weight_add")]
    public decimal? WeightAdd { get; set; }

    [JsonPropertyName("weight_mult")]
    public decimal? WeightMultiplier { get; set; }

    [JsonPropertyName("fault")]
    public string? Fault { get; set; }

    [JsonPropertyName("fault_group")]
    public string? FaultGroup { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
