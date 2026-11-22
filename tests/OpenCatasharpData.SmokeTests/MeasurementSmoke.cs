using System.Text.Json;
using OpenCatasharpData.Models;
using OpenCatasharpData.Parsing;

internal static class MeasurementSmoke
{
    public static void Run()
    {
        var volumeMl = JsonSerializer.Deserialize<CddaVolume>("\"250 ml\"");
        var volumeL = JsonSerializer.Deserialize<CddaVolume>("\"0.25 L\"");
        Check(volumeMl.Millilitres == 250m && volumeMl.Litres == 0.25m, "Volume conversion");
        Check(volumeMl == volumeL && volumeMl.GetHashCode() == volumeL.GetHashCode(),
            "Equivalent volumes have value equality");
        Check(JsonSerializer.Serialize(volumeL) == "\"0.25 L\"", "Volume JSON source round trip");

        var lengthCm = JsonSerializer.Deserialize<CddaLength>("\"15 cm\"");
        var lengthMm = CddaLength.FromMillimetres(150m);
        Check(lengthCm.Millimetres == 150m && lengthCm == lengthMm,
            "Length normalisation and value equality");
        Check(CddaLength.FromKilometres(1m) == CddaLength.FromMetres(1000m),
            "Kilometre unit found in real CDDA item data");
        Check(JsonSerializer.Serialize(CddaLength.FromMetres(1.25m)) == "\"1.25 m\"",
            "New length values use invariant formatting");

        var massKg = JsonSerializer.Deserialize<CddaMass>("\"1.5 kg\"");
        Check(massKg.Grams == 1500m && massKg == CddaMass.FromGrams(1500m),
            "Mass normalisation and value equality");
        var unqualified = JsonSerializer.Deserialize<CddaMass>("5");
        Check(!unqualified.HasExplicitUnit && unqualified.Milligrams is null &&
            unqualified.Value == 5m && JsonSerializer.Serialize(unqualified) == "5",
            "Unqualified JSON numbers do not silently acquire units");
        Check(JsonSerializer.Serialize(JsonSerializer.Deserialize<CddaMass>("\"5\"")) == "\"5\"",
            "Unqualified JSON string remains a string");

        var minutes = JsonSerializer.Deserialize<CddaDuration>("\"15 m\"");
        Check(minutes.Minutes == 15m && minutes.Seconds == 900m, "Duration 'm' means minutes");
        Check(JsonSerializer.Deserialize<CddaLength>("\"15 m\"").Metres == 15m,
            "Length 'm' means metres");
        var compound = JsonSerializer.Deserialize<CddaDuration>("\" +1 day -23 hours 50m \"");
        Check(compound.Seconds == 6600m && compound.Unit == CddaDurationUnit.Compound,
            "Signed compound durations");
        Check(JsonDocument.Parse(JsonSerializer.Serialize(compound)).RootElement.GetString() ==
            " +1 day -23 hours 50m ", "Compound duration JSON round trip");
        var turns = JsonSerializer.Deserialize<CddaDuration>("\"1 turn 1 minutes 9 turns\"");
        Check(turns.Seconds == 70m && turns.ToTimeSpan() == TimeSpan.FromSeconds(70),
            "CDDA turns are one second each");
        Check(JsonSerializer.Serialize(CddaDuration.FromDays(2m)) == "\"2 d\"",
            "Typed duration factory serialises correctly");

        var rejectedUnit = false;
        try { JsonSerializer.Deserialize<CddaVolume>("\"10 cm\""); }
        catch (JsonException) { rejectedUnit = true; }
        Check(rejectedUnit, "Dimension mismatch raises JsonException");

        var dto = CddaRecordParser.Default.Parse(
            """
            {"type":"ITEM","id":"measurements","weight":"1 kg","volume":"2 L",
             "longest_side":"25 cm","barrel_length":"100 mm",
             "integral_weight":"500 g","integral_volume":"15 ml",
             "pocket_data":[{"pocket_type":"CONTAINER","max_contains_volume":"1 L",
                             "max_contains_weight":"2 kg","max_item_length":"35 cm",
                             "magazine_well":"250 ml"}]}
            """) as GenericItemDefinition;
        Check(dto is not null && dto.Weight?.Grams == 1000m &&
            dto.Volume?.Millilitres == 2000m && dto.LongestSide?.Millimetres == 250m &&
            dto.BarrelLength?.Millimetres == 100m &&
            dto.IntegralWeight?.Grams == 500m && dto.IntegralVolume?.Millilitres == 15m,
            "Item dimensions eagerly deserialised into distinct value types");
        Check(dto.PocketData?[0].MaxContainsVolume?.Millilitres == 1000m &&
            dto.PocketData[0].MaxContainsWeight?.Grams == 2000m &&
            dto.PocketData[0].MaxItemLength?.Centimetres == 35m &&
            dto.PocketData[0].MagazineWell?.Millilitres == 250m,
            "Pocket dimensions eagerly typed");

        var food = CddaRecordParser.Default.Parse(
            """{"type":"ITEM","subtypes":["COMESTIBLE"],"id":"food","spoils_in":"6 days"}""")
            as ComestibleItemDefinition;
        Check(food?.SpoilsIn?.Days == 6m, "Spoilage is a typed duration");
        var gunmod = CddaRecordParser.Default.Parse(
            """{"type":"ITEM","subtypes":["GUNMOD"],"id":"attachment","install_time":"15 m"}""")
            as GunModItemDefinition;
        Check(gunmod?.InstallTime?.Minutes == 15m, "Gunmod installation duration typed");

        Console.WriteLine("Typed measurement value tests passed.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("FAILED: " + message);
    }
}
