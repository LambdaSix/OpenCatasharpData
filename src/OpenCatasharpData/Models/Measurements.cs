using System.Globalization;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>Source unit for a volume; Unspecified represents a bare JSON number.</summary>
public enum CddaVolumeUnit { Unspecified, Millilitres, Litres }

/// <summary>Source unit for a length; Unspecified represents a bare JSON number.</summary>
public enum CddaLengthUnit { Unspecified, Millimetres, Centimetres, Metres, Kilometres }

/// <summary>Source unit for a mass; Unspecified represents a bare JSON number.</summary>
public enum CddaMassUnit { Unspecified, Milligrams, Grams, Kilograms }

/// <summary>Source unit for a duration. Compound represents multiple signed components.</summary>
public enum CddaDurationUnit { Unspecified, Seconds, Turns, Minutes, Hours, Days, Weeks, Compound }

/// <summary>
/// Strongly typed CDDA volume. Explicit units are normalised eagerly to millilitres.
/// A unitless JSON number retains its numeric value without guessing its unit.
/// </summary>
[JsonConverter(typeof(CddaVolumeJsonConverter))]
public readonly record struct CddaVolume
{
    internal CddaVolume(decimal value, CddaVolumeUnit unit, string? originalText = null)
    {
        Value = value;
        Unit = unit;
        OriginalText = originalText;
    }

    public decimal Value { get; }
    public CddaVolumeUnit Unit { get; }
    public string? OriginalText { get; }
    public bool HasExplicitUnit => Unit != CddaVolumeUnit.Unspecified;
    public decimal? Millilitres => Unit switch
    {
        CddaVolumeUnit.Millilitres => Value,
        CddaVolumeUnit.Litres => Value * 1000m,
        _ => null
    };
    public decimal? Litres => Millilitres / 1000m;
    public static CddaVolume FromMillilitres(decimal value) => new(value, CddaVolumeUnit.Millilitres);
    public static CddaVolume FromLitres(decimal value) => new(value, CddaVolumeUnit.Litres);

    public bool Equals(CddaVolume other) =>
        HasExplicitUnit == other.HasExplicitUnit &&
        (HasExplicitUnit ? Millilitres == other.Millilitres : Value == other.Value);
    public override int GetHashCode() =>
        HasExplicitUnit ? HashCode.Combine(1, Millilitres) : HashCode.Combine(0, Value);
    public override string ToString() => OriginalText ??
        (Value.ToString(CultureInfo.InvariantCulture) + (Unit switch
        {
            CddaVolumeUnit.Millilitres => " ml",
            CddaVolumeUnit.Litres => " L",
            _ => ""
        }));
}

/// <summary>
/// Strongly typed CDDA length. Explicit units are normalised eagerly to millimetres.
/// </summary>
[JsonConverter(typeof(CddaLengthJsonConverter))]
public readonly record struct CddaLength
{
    internal CddaLength(decimal value, CddaLengthUnit unit, string? originalText = null)
    {
        Value = value;
        Unit = unit;
        OriginalText = originalText;
    }

    public decimal Value { get; }
    public CddaLengthUnit Unit { get; }
    public string? OriginalText { get; }
    public bool HasExplicitUnit => Unit != CddaLengthUnit.Unspecified;
    public decimal? Millimetres => Unit switch
    {
        CddaLengthUnit.Millimetres => Value,
        CddaLengthUnit.Centimetres => Value * 10m,
        CddaLengthUnit.Metres => Value * 1000m,
        CddaLengthUnit.Kilometres => Value * 1000000m,
        _ => null
    };
    public decimal? Centimetres => Millimetres / 10m;
    public decimal? Metres => Millimetres / 1000m;
    public decimal? Kilometres => Millimetres / 1000000m;
    public static CddaLength FromMillimetres(decimal value) => new(value, CddaLengthUnit.Millimetres);
    public static CddaLength FromCentimetres(decimal value) => new(value, CddaLengthUnit.Centimetres);
    public static CddaLength FromMetres(decimal value) => new(value, CddaLengthUnit.Metres);
    public static CddaLength FromKilometres(decimal value) => new(value, CddaLengthUnit.Kilometres);

    public bool Equals(CddaLength other) =>
        HasExplicitUnit == other.HasExplicitUnit &&
        (HasExplicitUnit ? Millimetres == other.Millimetres : Value == other.Value);
    public override int GetHashCode() =>
        HasExplicitUnit ? HashCode.Combine(1, Millimetres) : HashCode.Combine(0, Value);
    public override string ToString() => OriginalText ??
        (Value.ToString(CultureInfo.InvariantCulture) + (Unit switch
        {
            CddaLengthUnit.Millimetres => " mm",
            CddaLengthUnit.Centimetres => " cm",
            CddaLengthUnit.Metres => " m",
            CddaLengthUnit.Kilometres => " km",
            _ => ""
        }));
}

/// <summary>
/// Strongly typed CDDA mass. Explicit units are normalised eagerly to milligrams.
/// </summary>
[JsonConverter(typeof(CddaMassJsonConverter))]
public readonly record struct CddaMass
{
    internal CddaMass(decimal value, CddaMassUnit unit, string? originalText = null)
    {
        Value = value;
        Unit = unit;
        OriginalText = originalText;
    }

    public decimal Value { get; }
    public CddaMassUnit Unit { get; }
    public string? OriginalText { get; }
    public bool HasExplicitUnit => Unit != CddaMassUnit.Unspecified;
    public decimal? Milligrams => Unit switch
    {
        CddaMassUnit.Milligrams => Value,
        CddaMassUnit.Grams => Value * 1000m,
        CddaMassUnit.Kilograms => Value * 1000000m,
        _ => null
    };
    public decimal? Grams => Milligrams / 1000m;
    public decimal? Kilograms => Milligrams / 1000000m;
    public static CddaMass FromMilligrams(decimal value) => new(value, CddaMassUnit.Milligrams);
    public static CddaMass FromGrams(decimal value) => new(value, CddaMassUnit.Grams);
    public static CddaMass FromKilograms(decimal value) => new(value, CddaMassUnit.Kilograms);

    public bool Equals(CddaMass other) =>
        HasExplicitUnit == other.HasExplicitUnit &&
        (HasExplicitUnit ? Milligrams == other.Milligrams : Value == other.Value);
    public override int GetHashCode() =>
        HasExplicitUnit ? HashCode.Combine(1, Milligrams) : HashCode.Combine(0, Value);
    public override string ToString() => OriginalText ??
        (Value.ToString(CultureInfo.InvariantCulture) + (Unit switch
        {
            CddaMassUnit.Milligrams => " mg",
            CddaMassUnit.Grams => " g",
            CddaMassUnit.Kilograms => " kg",
            _ => ""
        }));
}

/// <summary>
/// CDDA duration in seconds (one CDDA turn = one second).
/// Supports signed compound durations such as "+1 day -23 hours 50m".
/// Bare numeric JSON values are retained without assigning an implicit unit.
/// </summary>
[JsonConverter(typeof(CddaDurationJsonConverter))]
public readonly record struct CddaDuration
{
    internal CddaDuration(decimal? seconds, decimal? unqualified, CddaDurationUnit unit, string? originalText)
    {
        Seconds = seconds;
        UnqualifiedValue = unqualified;
        Unit = unit;
        OriginalText = originalText;
    }

    public decimal? Seconds { get; }
    public decimal? UnqualifiedValue { get; }
    public CddaDurationUnit Unit { get; }
    public string? OriginalText { get; }
    public bool HasExplicitUnit => Unit != CddaDurationUnit.Unspecified;
    public decimal? Minutes => Seconds / 60m;
    public decimal? Hours => Seconds / 3600m;
    public decimal? Days => Seconds / 86400m;
    public TimeSpan? ToTimeSpan() => Seconds is { } s
        ? TimeSpan.FromSeconds((double)s) : null;
    public static CddaDuration FromSeconds(decimal value) =>
        new(value, null, CddaDurationUnit.Seconds, null);
    public static CddaDuration FromMinutes(decimal value) =>
        new(value * 60m, null, CddaDurationUnit.Minutes, null);
    public static CddaDuration FromHours(decimal value) =>
        new(value * 3600m, null, CddaDurationUnit.Hours, null);
    public static CddaDuration FromDays(decimal value) =>
        new(value * 86400m, null, CddaDurationUnit.Days, null);

    public bool Equals(CddaDuration other) =>
        HasExplicitUnit == other.HasExplicitUnit &&
        (HasExplicitUnit ? Seconds == other.Seconds : UnqualifiedValue == other.UnqualifiedValue);
    public override int GetHashCode() =>
        HasExplicitUnit ? HashCode.Combine(1, Seconds) : HashCode.Combine(0, UnqualifiedValue);
    public override string ToString() => OriginalText ??
        (Unit == CddaDurationUnit.Unspecified
            ? UnqualifiedValue?.ToString(CultureInfo.InvariantCulture) ?? "0"
            : (Unit switch
            {
                CddaDurationUnit.Minutes => (Seconds / 60m)?.ToString(CultureInfo.InvariantCulture) + " m",
                CddaDurationUnit.Hours => (Seconds / 3600m)?.ToString(CultureInfo.InvariantCulture) + " h",
                CddaDurationUnit.Days => (Seconds / 86400m)?.ToString(CultureInfo.InvariantCulture) + " d",
                CddaDurationUnit.Weeks => (Seconds / 604800m)?.ToString(CultureInfo.InvariantCulture) + " weeks",
                CddaDurationUnit.Turns => Seconds?.ToString(CultureInfo.InvariantCulture) + " turns",
                _ => Seconds?.ToString(CultureInfo.InvariantCulture) + " s"
            }));
}
