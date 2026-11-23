using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>Units of ammunition/energy consumed from a named pocket when a firing mode is used.</summary>
public sealed class CddaFiringCost
{
    [JsonPropertyName("pocket")]
    public string? Pocket { get; set; }

    [JsonPropertyName("qty")]
    public int? Quantity { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Change one named pocket's firing cost or ammunition capacity.</summary>
public sealed class CddaPocketModifier
{
    [JsonPropertyName("pocket")]
    public string? Pocket { get; set; }

    [JsonPropertyName("set")]
    public int? Set { get; set; }

    [JsonPropertyName("multiply")]
    public decimal? Multiply { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>CDDA gunmod magazine adapter tuple: ["ammo type", ["magazine ids"]].</summary>
[JsonConverter(typeof(CddaMagazineAdaptorJsonConverter))]
public sealed class CddaMagazineAdaptor
{
    public string AmmoType { get; set; } = string.Empty;
    public List<string> Magazines { get; set; } = [];
}

public sealed class CddaMagazineAdaptorJsonConverter : JsonConverter<CddaMagazineAdaptor>
{
    public override CddaMagazineAdaptor? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var element = doc.RootElement;
        if (element.ValueKind == JsonValueKind.Null) return null;
        if (element.ValueKind != JsonValueKind.Array || element.GetArrayLength() != 2 ||
            element[0].ValueKind != JsonValueKind.String || element[1].ValueKind != JsonValueKind.Array)
            throw new JsonException("Magazine adapter must be [ammoType, [magazineIds]].");

        return new CddaMagazineAdaptor
        {
            AmmoType = element[0].GetString()!,
            Magazines = element[1].EnumerateArray().Select(mag =>
                mag.ValueKind == JsonValueKind.String
                    ? mag.GetString()!
                    : throw new JsonException("Magazine ID must be a string.")).ToList()
        };
    }

    public override void Write(Utf8JsonWriter writer, CddaMagazineAdaptor value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.AmmoType);
        JsonSerializer.Serialize(writer, value.Magazines, options);
        writer.WriteEndArray();
    }
}

/// <summary>One barrel length to dispersion mapping for AMMO dispersion_modifier.</summary>
public sealed class CddaBarrelDispersion
{
    [JsonPropertyName("barrel_length")]
    public CddaLength? BarrelLength { get; set; }

    [JsonPropertyName("dispersion")]
    public decimal? Dispersion { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>
/// Ammo dispersion_modifier is either a numeric value or barrel-length list.
/// Source JSON shape is preserved for round-trip serialization.
/// </summary>
[JsonConverter(typeof(CddaAmmoDispersionJsonConverter))]
public sealed class CddaAmmoDispersion
{
    public decimal? Flat { get; set; }
    public List<CddaBarrelDispersion>? ByBarrel { get; set; }
}

public sealed class CddaAmmoDispersionJsonConverter : JsonConverter<CddaAmmoDispersion>
{
    public override CddaAmmoDispersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        return root.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.Number => new CddaAmmoDispersion { Flat = root.GetDecimal() },
            JsonValueKind.Array => new CddaAmmoDispersion
            {
                ByBarrel = root.Deserialize<List<CddaBarrelDispersion>>(options)
            },
            _ => throw new JsonException("Expected numeric or barrel-array dispersion_modifier.")
        };
    }

    public override void Write(Utf8JsonWriter writer, CddaAmmoDispersion value, JsonSerializerOptions options)
    {
        if (value.ByBarrel is not null)
            JsonSerializer.Serialize(writer, value.ByBarrel, options);
        else if (value.Flat is { } flat)
            writer.WriteNumberValue(flat);
        else writer.WriteNullValue();
    }
}

/// <summary>
/// Optional shrapnel characteristics in an explosion. A bare JSON number
/// (usually zero) is a documented legacy shorthand and is preserved.
/// </summary>
[JsonConverter(typeof(CddaShrapnelJsonConverter))]
public sealed class CddaShrapnel
{
    [JsonIgnore]
    public decimal? LegacyValue { get; internal set; }

    [JsonPropertyName("casing_mass")]
    public decimal? CasingMass { get; set; }

    [JsonPropertyName("fragment_mass")]
    public decimal? FragmentMass { get; set; }

    [JsonPropertyName("recovery")]
    public decimal? Recovery { get; set; }

    [JsonPropertyName("drop")]
    public string? Drop { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Round-trips numeric or object-shaped explosion shrapnel.</summary>
public sealed class CddaShrapnelJsonConverter : JsonConverter<CddaShrapnel>
{
    public override CddaShrapnel? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Null) return null;
        if (root.ValueKind == JsonValueKind.Number)
            return new CddaShrapnel { LegacyValue = root.GetDecimal() };
        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException("Expected shrapnel details or numeric shorthand.");

        var result = new CddaShrapnel();
        foreach (var prop in root.EnumerateObject())
        {
            switch (prop.Name)
            {
                case "casing_mass": result.CasingMass = prop.Value.GetDecimal(); break;
                case "fragment_mass": result.FragmentMass = prop.Value.GetDecimal(); break;
                case "recovery": result.Recovery = prop.Value.GetDecimal(); break;
                case "drop": result.Drop = prop.Value.GetString(); break;
                default:
                    (result.AdditionalProperties ??= new())[prop.Name] = prop.Value.Clone();
                    break;
            }
        }
        return result;
    }

    public override void Write(
        Utf8JsonWriter writer, CddaShrapnel value, JsonSerializerOptions options)
    {
        if (value.LegacyValue is { } raw)
        {
            writer.WriteNumberValue(raw);
            return;
        }

        writer.WriteStartObject();
        if (value.CasingMass is { } casing) writer.WriteNumber("casing_mass", casing);
        if (value.FragmentMass is { } fragment) writer.WriteNumber("fragment_mass", fragment);
        if (value.Recovery is { } recovery) writer.WriteNumber("recovery", recovery);
        if (value.Drop is not null) writer.WriteString("drop", value.Drop);
        if (value.AdditionalProperties is not null)
            foreach (var pair in value.AdditionalProperties)
            {
                writer.WritePropertyName(pair.Key);
                pair.Value.WriteTo(writer);
            }
        writer.WriteEndObject();
    }
}

/// <summary>Explosion produced by an ammunition item; unknown effect parameters remain intact.</summary>
public sealed class CddaExplosion
{
    [JsonPropertyName("power")]
    public decimal? Power { get; set; }

    [JsonPropertyName("distance_factor")]
    public decimal? DistanceFactor { get; set; }

    [JsonPropertyName("fire")]
    public bool? Fire { get; set; }

    [JsonPropertyName("shrapnel")]
    public CddaShrapnel? Shrapnel { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
