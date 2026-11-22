using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenCatasharpData.Models;

/// <summary>Independent parser for one numeric quantity plus its unit token.</summary>
internal static class CddaUnitReader
{
    internal static (decimal Amount, string Unit) Single(string source)
    {
        var span = source.AsSpan().Trim();
        if (span.IsEmpty) throw new JsonException("Empty measurement.");

        var i = 0;
        if (span[i] is '+' or '-') i++;
        var hasDigits = false;
        while (i < span.Length && char.IsDigit(span[i]))
        {
            i++;
            hasDigits = true;
        }
        if (i < span.Length && span[i] == '.')
        {
            i++;
            while (i < span.Length && char.IsDigit(span[i]))
            {
                i++;
                hasDigits = true;
            }
        }
        if (!hasDigits) throw new JsonException($"Missing measurement value: '{source}'.");
        if (i < span.Length && span[i] is 'e' or 'E')
        {
            var exponent = i++;
            if (i < span.Length && span[i] is '+' or '-') i++;
            var digitStart = i;
            while (i < span.Length && char.IsDigit(span[i])) i++;
            if (i == digitStart) i = exponent;
        }

        if (!decimal.TryParse(span[..i], NumberStyles.Float, CultureInfo.InvariantCulture, out var amount))
            throw new JsonException($"Invalid measurement value: '{source}'.");
        var unit = span[i..].Trim().ToString().ToLowerInvariant();
        return (amount, unit);
    }

    internal static JsonException Unsupported(string dimension, string unit) =>
        new($"Unsupported {dimension} unit '{unit}'.");
}

public sealed class CddaVolumeJsonConverter : JsonConverter<CddaVolume>
{
    public override CddaVolume Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return new CddaVolume(reader.GetDecimal(), CddaVolumeUnit.Unspecified);
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Volume must be a number or a unit-bearing string.");

        var raw = reader.GetString()!;
        var (amount, suffix) = CddaUnitReader.Single(raw);
        var unit = suffix switch
        {
            "ml" or "milliliter" or "milliliters" or "millilitre" or "millilitres" =>
                CddaVolumeUnit.Millilitres,
            "l" or "liter" or "liters" or "litre" or "litres" =>
                CddaVolumeUnit.Litres,
            "" => CddaVolumeUnit.Unspecified,
            _ => throw CddaUnitReader.Unsupported("volume", suffix)
        };
        return new CddaVolume(amount, unit, raw);
    }

    public override void Write(Utf8JsonWriter writer, CddaVolume value, JsonSerializerOptions options)
    {
        if (value.Unit == CddaVolumeUnit.Unspecified && value.OriginalText is null)
            writer.WriteNumberValue(value.Value);
        else writer.WriteStringValue(value.ToString());
    }
}

public sealed class CddaLengthJsonConverter : JsonConverter<CddaLength>
{
    public override CddaLength Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return new CddaLength(reader.GetDecimal(), CddaLengthUnit.Unspecified);
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Length must be a number or a unit-bearing string.");

        var raw = reader.GetString()!;
        var (amount, suffix) = CddaUnitReader.Single(raw);
        var unit = suffix switch
        {
            "mm" or "millimeter" or "millimeters" or "millimetre" or "millimetres" =>
                CddaLengthUnit.Millimetres,
            "cm" or "centimeter" or "centimeters" or "centimetre" or "centimetres" =>
                CddaLengthUnit.Centimetres,
            "m" or "meter" or "meters" or "metre" or "metres" =>
                CddaLengthUnit.Metres,
            "" => CddaLengthUnit.Unspecified,
            _ => throw CddaUnitReader.Unsupported("length", suffix)
        };
        return new CddaLength(amount, unit, raw);
    }

    public override void Write(Utf8JsonWriter writer, CddaLength value, JsonSerializerOptions options)
    {
        if (value.Unit == CddaLengthUnit.Unspecified && value.OriginalText is null)
            writer.WriteNumberValue(value.Value);
        else writer.WriteStringValue(value.ToString());
    }
}

public sealed class CddaMassJsonConverter : JsonConverter<CddaMass>
{
    public override CddaMass Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return new CddaMass(reader.GetDecimal(), CddaMassUnit.Unspecified);
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Mass must be a number or a unit-bearing string.");

        var raw = reader.GetString()!;
        var (amount, suffix) = CddaUnitReader.Single(raw);
        var unit = suffix switch
        {
            "mg" or "milligram" or "milligrams" => CddaMassUnit.Milligrams,
            "g" or "gram" or "grams" => CddaMassUnit.Grams,
            "kg" or "kilogram" or "kilograms" => CddaMassUnit.Kilograms,
            "" => CddaMassUnit.Unspecified,
            _ => throw CddaUnitReader.Unsupported("mass", suffix)
        };
        return new CddaMass(amount, unit, raw);
    }

    public override void Write(Utf8JsonWriter writer, CddaMass value, JsonSerializerOptions options)
    {
        if (value.Unit == CddaMassUnit.Unspecified && value.OriginalText is null)
            writer.WriteNumberValue(value.Value);
        else writer.WriteStringValue(value.ToString());
    }
}

public sealed class CddaDurationJsonConverter : JsonConverter<CddaDuration>
{
    public override CddaDuration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return new CddaDuration(null, reader.GetDecimal(), CddaDurationUnit.Unspecified, null);
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Duration must be a number or a unit-bearing string.");
        var raw = reader.GetString()!;
        var span = raw.AsSpan();
        decimal seconds = 0;
        var count = 0;
        var last = CddaDurationUnit.Unspecified;
        var position = 0;

        while (position < span.Length)
        {
            while (position < span.Length && char.IsWhiteSpace(span[position])) position++;
            if (position == span.Length) break;

            var start = position;
            if (span[position] is '+' or '-') position++;
            var hasDigits = false;
            while (position < span.Length && char.IsDigit(span[position]))
            {
                hasDigits = true;
                position++;
            }
            if (position < span.Length && span[position] == '.')
            {
                position++;
                while (position < span.Length && char.IsDigit(span[position]))
                {
                    hasDigits = true;
                    position++;
                }
            }
            if (!hasDigits || !decimal.TryParse(
                    span[start..position], NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out var amount))
                throw new JsonException($"Invalid CDDA duration: '{raw}'.");

            while (position < span.Length && char.IsWhiteSpace(span[position])) position++;
            var unitStart = position;
            while (position < span.Length && char.IsLetter(span[position])) position++;
            var token = span[unitStart..position].ToString().ToLowerInvariant();

            var (unit, factor) = token switch
            {
                "ms" or "millisecond" or "milliseconds" => (CddaDurationUnit.Seconds, 0.001m),
                "s" or "sec" or "secs" or "second" or "seconds" =>
                    (CddaDurationUnit.Seconds, 1m),
                "t" or "turn" or "turns" => (CddaDurationUnit.Turns, 1m),
                "m" or "min" or "mins" or "minute" or "minutes" =>
                    (CddaDurationUnit.Minutes, 60m),
                "h" or "hr" or "hrs" or "hour" or "hours" =>
                    (CddaDurationUnit.Hours, 3600m),
                "d" or "day" or "days" => (CddaDurationUnit.Days, 86400m),
                "w" or "week" or "weeks" => (CddaDurationUnit.Weeks, 604800m),
                "" when count == 0 && position == span.Length =>
                    (CddaDurationUnit.Unspecified, 1m),
                _ => throw CddaUnitReader.Unsupported("duration", token)
            };
            if (unit == CddaDurationUnit.Unspecified)
                return new CddaDuration(null, amount, unit, raw);

            seconds += amount * factor;
            last = unit;
            count++;
        }

        if (count == 0) throw new JsonException("Duration cannot be empty.");
        return new CddaDuration(
            seconds, null, count == 1 ? last : CddaDurationUnit.Compound, raw);
    }

    public override void Write(Utf8JsonWriter writer, CddaDuration value, JsonSerializerOptions options)
    {
        if (value.Unit == CddaDurationUnit.Unspecified && value.OriginalText is null)
            writer.WriteNumberValue(value.UnqualifiedValue ?? 0);
        else writer.WriteStringValue(value.ToString());
    }
}
