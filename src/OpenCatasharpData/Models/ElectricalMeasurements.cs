using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace OpenCatasharpData.Models;

/// <summary>Source unit of an energy quantity. Unspecified means the JSON value has no explicit unit.</summary>
public enum CddaEnergyUnit
{
    Unspecified, Millijoules, Joules, Kilojoules, Megajoules, Gigajoules,
    WattHours, KilowattHours
}

/// <summary>Source unit of an electrical or mechanical power quantity.</summary>
public enum CddaPowerUnit
{
    Unspecified, Milliwatts, Watts, Kilowatts, Megawatts
}

/// <summary>
/// Dimension-safe energy quantity. Explicit units are eagerly normalised to joules.
/// Unitless JSON numbers stay unitless, including during JSON round-trips.
/// </summary>
[JsonConverter(typeof(CddaEnergyJsonConverter))]
public readonly record struct CddaEnergy
{
    internal CddaEnergy(decimal amount, CddaEnergyUnit unit, string? source = null)
    {
        Value = amount;
        Unit = unit;
        OriginalText = source;
    }

    public decimal Value { get; }
    public CddaEnergyUnit Unit { get; }
    public string? OriginalText { get; }
    public bool HasExplicitUnit => Unit != CddaEnergyUnit.Unspecified;

    public decimal? Joules => Unit switch
    {
        CddaEnergyUnit.Millijoules => Value / 1000m,
        CddaEnergyUnit.Joules => Value,
        CddaEnergyUnit.Kilojoules => Value * 1000m,
        CddaEnergyUnit.Megajoules => Value * 1000000m,
        CddaEnergyUnit.Gigajoules => Value * 1000000000m,
        CddaEnergyUnit.WattHours => Value * 3600m,
        CddaEnergyUnit.KilowattHours => Value * 3600000m,
        _ => null
    };

    public decimal? Kilojoules => Joules / 1000m;
    public decimal? WattHours => Joules / 3600m;

    public static CddaEnergy FromJoules(decimal value) => new(value, CddaEnergyUnit.Joules);
    public static CddaEnergy FromKilojoules(decimal value) => new(value, CddaEnergyUnit.Kilojoules);
    public static CddaEnergy FromWattHours(decimal value) => new(value, CddaEnergyUnit.WattHours);

    public bool Equals(CddaEnergy other) =>
        HasExplicitUnit == other.HasExplicitUnit &&
        (HasExplicitUnit ? Joules == other.Joules : Value == other.Value);
    public override int GetHashCode() =>
        HasExplicitUnit ? HashCode.Combine(1, Joules) : HashCode.Combine(0, Value);

    public override string ToString() => OriginalText ??
        Value.ToString(CultureInfo.InvariantCulture) + (Unit switch
        {
            CddaEnergyUnit.Millijoules => " mJ",
            CddaEnergyUnit.Joules => " J",
            CddaEnergyUnit.Kilojoules => " kJ",
            CddaEnergyUnit.Megajoules => " MJ",
            CddaEnergyUnit.Gigajoules => " GJ",
            CddaEnergyUnit.WattHours => " Wh",
            CddaEnergyUnit.KilowattHours => " kWh",
            _ => ""
        });
}

/// <summary>
/// Dimension-safe rate of energy consumption or production, in watts.
/// Milliwatts and megawatts remain distinct (case-sensitive SI prefixes).
/// </summary>
[JsonConverter(typeof(CddaPowerJsonConverter))]
public readonly record struct CddaPower
{
    internal CddaPower(decimal amount, CddaPowerUnit unit, string? source = null)
    {
        Value = amount;
        Unit = unit;
        OriginalText = source;
    }

    public decimal Value { get; }
    public CddaPowerUnit Unit { get; }
    public string? OriginalText { get; }
    public bool HasExplicitUnit => Unit != CddaPowerUnit.Unspecified;

    public decimal? Watts => Unit switch
    {
        CddaPowerUnit.Milliwatts => Value / 1000m,
        CddaPowerUnit.Watts => Value,
        CddaPowerUnit.Kilowatts => Value * 1000m,
        CddaPowerUnit.Megawatts => Value * 1000000m,
        _ => null
    };

    public decimal? Kilowatts => Watts / 1000m;

    public static CddaPower FromWatts(decimal value) => new(value, CddaPowerUnit.Watts);
    public static CddaPower FromKilowatts(decimal value) => new(value, CddaPowerUnit.Kilowatts);

    public bool Equals(CddaPower other) =>
        HasExplicitUnit == other.HasExplicitUnit &&
        (HasExplicitUnit ? Watts == other.Watts : Value == other.Value);
    public override int GetHashCode() =>
        HasExplicitUnit ? HashCode.Combine(1, Watts) : HashCode.Combine(0, Value);
    public override string ToString() => OriginalText ??
        Value.ToString(CultureInfo.InvariantCulture) + (Unit switch
        {
            CddaPowerUnit.Milliwatts => " mW",
            CddaPowerUnit.Watts => " W",
            CddaPowerUnit.Kilowatts => " kW",
            CddaPowerUnit.Megawatts => " MW",
            _ => ""
        });
}

/// <summary>Case-sensitive quantity parsing: SI 'm' and 'M' must not be conflated.</summary>
internal static class CddaElectricalUnitReader
{
    private static readonly Regex Expression = new(
        @"^\s*([+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[Ee][+-]?\d+)?)\s*([A-Za-z]*)\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static (decimal Amount, string Unit) Read(string text)
    {
        var match = Expression.Match(text);
        if (!match.Success ||
            !decimal.TryParse(match.Groups[1].Value, NumberStyles.Float,
                CultureInfo.InvariantCulture, out var amount))
            throw new JsonException($"Invalid energy/power quantity '{text}'.");
        return (amount, match.Groups[2].Value);
    }
}

public sealed class CddaEnergyJsonConverter : JsonConverter<CddaEnergy>
{
    public override CddaEnergy Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return new CddaEnergy(reader.GetDecimal(), CddaEnergyUnit.Unspecified);
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Energy must be a number or a unit-bearing string.");

        var original = reader.GetString()!;
        var (amount, suffix) = CddaElectricalUnitReader.Read(original);
        var unit = suffix switch
        {
            "" => CddaEnergyUnit.Unspecified,
            "mJ" => CddaEnergyUnit.Millijoules,
            "J" => CddaEnergyUnit.Joules,
            "kJ" => CddaEnergyUnit.Kilojoules,
            "MJ" => CddaEnergyUnit.Megajoules,
            "GJ" => CddaEnergyUnit.Gigajoules,
            "Wh" => CddaEnergyUnit.WattHours,
            "kWh" => CddaEnergyUnit.KilowattHours,
            _ => throw new JsonException($"Unsupported energy unit '{suffix}'.")
        };
        return new CddaEnergy(amount, unit, original);
    }

    public override void Write(Utf8JsonWriter writer, CddaEnergy value, JsonSerializerOptions options)
    {
        if (value.Unit == CddaEnergyUnit.Unspecified && value.OriginalText is null)
            writer.WriteNumberValue(value.Value);
        else writer.WriteStringValue(value.ToString());
    }
}

public sealed class CddaPowerJsonConverter : JsonConverter<CddaPower>
{
    public override CddaPower Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return new CddaPower(reader.GetDecimal(), CddaPowerUnit.Unspecified);
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Power must be a number or a unit-bearing string.");

        var original = reader.GetString()!;
        var (amount, suffix) = CddaElectricalUnitReader.Read(original);
        var unit = suffix switch
        {
            "" => CddaPowerUnit.Unspecified,
            "mW" => CddaPowerUnit.Milliwatts,
            "W" => CddaPowerUnit.Watts,
            "kW" => CddaPowerUnit.Kilowatts,
            "MW" => CddaPowerUnit.Megawatts,
            _ => throw new JsonException($"Unsupported power unit '{suffix}'.")
        };
        return new CddaPower(amount, unit, original);
    }

    public override void Write(Utf8JsonWriter writer, CddaPower value, JsonSerializerOptions options)
    {
        if (value.Unit == CddaPowerUnit.Unspecified && value.OriginalText is null)
            writer.WriteNumberValue(value.Value);
        else writer.WriteStringValue(value.ToString());
    }
}
