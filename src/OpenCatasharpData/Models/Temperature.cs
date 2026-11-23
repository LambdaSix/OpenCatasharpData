using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace OpenCatasharpData.Models;

/// <summary>Unit in which a CDDA absolute temperature was written.</summary>
public enum CddaTemperatureUnit
{
    Unspecified,
    Celsius,
    MilliCelsius,
    Fahrenheit,
    Kelvin
}

/// <summary>
/// Eagerly parsed absolute temperature. The canonical quantity is degrees Celsius.
/// Bare numeric values retain their unqualified shape instead of assuming Celsius.
/// For temperature *differences*, use a separate quantity rather than applying
/// the affine Fahrenheit/Kelvin conversion to a temperature delta.
/// </summary>
[JsonConverter(typeof(CddaTemperatureJsonConverter))]
public readonly record struct CddaTemperature
{
    internal CddaTemperature(decimal value, CddaTemperatureUnit unit, string? originalText = null)
    {
        Value = value;
        Unit = unit;
        OriginalText = originalText;
    }

    public decimal Value { get; }
    public CddaTemperatureUnit Unit { get; }
    public string? OriginalText { get; }
    public bool HasExplicitUnit => Unit != CddaTemperatureUnit.Unspecified;

    public decimal? Celsius => Unit switch
    {
        CddaTemperatureUnit.Celsius => Value,
        CddaTemperatureUnit.MilliCelsius => Value / 1000m,
        CddaTemperatureUnit.Fahrenheit => (Value - 32m) * 5m / 9m,
        CddaTemperatureUnit.Kelvin => Value - 273.15m,
        _ => null
    };

    public decimal? MilliCelsius => Celsius * 1000m;
    public decimal? Fahrenheit => Celsius * 9m / 5m + 32m;
    public decimal? Kelvin => Celsius + 273.15m;

    public static CddaTemperature FromCelsius(decimal amount) =>
        new(amount, CddaTemperatureUnit.Celsius);
    public static CddaTemperature FromMilliCelsius(decimal amount) =>
        new(amount, CddaTemperatureUnit.MilliCelsius);
    public static CddaTemperature FromFahrenheit(decimal amount) =>
        new(amount, CddaTemperatureUnit.Fahrenheit);
    public static CddaTemperature FromKelvin(decimal amount) =>
        new(amount, CddaTemperatureUnit.Kelvin);

    public bool Equals(CddaTemperature other) =>
        HasExplicitUnit == other.HasExplicitUnit &&
        (HasExplicitUnit ? Celsius == other.Celsius : Value == other.Value);

    public override int GetHashCode() =>
        HasExplicitUnit ? HashCode.Combine(1, Celsius) : HashCode.Combine(0, Value);

    public override string ToString() => OriginalText ??
        Value.ToString(CultureInfo.InvariantCulture) + (Unit switch
        {
            CddaTemperatureUnit.Celsius => " C",
            CddaTemperatureUnit.MilliCelsius => " mC",
            CddaTemperatureUnit.Fahrenheit => " F",
            CddaTemperatureUnit.Kelvin => " K",
            _ => ""
        });
}

/// <summary>
/// Case-sensitive absolute temperature converter. In particular, 'mC' is
/// milli-Celsius, not Celsius multiplied by a duration or a current value.
/// </summary>
public sealed class CddaTemperatureJsonConverter : JsonConverter<CddaTemperature>
{
    private static readonly Regex Expression = new(
        @"^\s*([+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[Ee][+-]?\d+)?)\s*([A-Za-z°]*)\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public override CddaTemperature Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return new CddaTemperature(reader.GetDecimal(), CddaTemperatureUnit.Unspecified);
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Temperature must be a number or a unit-bearing string.");

        var original = reader.GetString()!;
        var match = Expression.Match(original);
        if (!match.Success ||
            !decimal.TryParse(match.Groups[1].Value, NumberStyles.Float,
                CultureInfo.InvariantCulture, out var amount))
            throw new JsonException($"Invalid CDDA temperature '{original}'.");

        var unit = match.Groups[2].Value switch
        {
            "" => CddaTemperatureUnit.Unspecified,
            "C" or "°C" or "celsius" or "Celsius" =>
                CddaTemperatureUnit.Celsius,
            "mC" or "millicelsius" or "milliCelsius" =>
                CddaTemperatureUnit.MilliCelsius,
            "F" or "°F" or "fahrenheit" or "Fahrenheit" =>
                CddaTemperatureUnit.Fahrenheit,
            "K" or "kelvin" or "Kelvin" =>
                CddaTemperatureUnit.Kelvin,
            var suffix => throw new JsonException($"Unsupported temperature unit '{suffix}'.")
        };
        return new CddaTemperature(amount, unit, original);
    }

    public override void Write(Utf8JsonWriter writer,
        CddaTemperature value, JsonSerializerOptions options)
    {
        if (value.Unit == CddaTemperatureUnit.Unspecified && value.OriginalText is null)
            writer.WriteNumberValue(value.Value);
        else writer.WriteStringValue(value.ToString());
    }
}
