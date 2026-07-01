using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;

// --- malý soubor: dvojí měření (studený vs. teplý běh) ---
var small = Load("../../data/test1.gpx", "1. načtení (studené)");
Load("../../data/test1.gpx", "2. načtení (teplé)");
PrintTracks(small);

// --- velký, reálný soubor ---
var big = Load("../../data/etapa01_den02.gpx", "Reálný soubor");
Console.WriteLine($"Celkem bodů: {big.Tracks.Sum(t => t.TrackSegments.Sum(s => s.Points.Count))}");

// Načte soubor, změří dobu a vypíše ji. Vrací načtená data.
static Gpx Load(string path, string label)
{
    var sw = Stopwatch.StartNew();
    var gpx = GpxReader.Load(path);
    sw.Stop();
    Console.WriteLine($"{label}: {sw.Elapsed.TotalMilliseconds:F3} ms");
    return gpx;
}

static void PrintTracks(Gpx gpx)
{
    Console.WriteLine($"Creator: {gpx.Creator}");
    foreach (var track in gpx.Tracks)
    {
        Console.WriteLine($"Trasa: {track.Name}");
        foreach (var segment in track.TrackSegments)
        {
            Console.WriteLine($"  Segment: {segment.Points.Count} bodů");
            foreach (var point in segment.Points)
                Console.WriteLine($"    {point.Latitude}, {point.Longitude} " +
                                  $"ele={point.Elevation} time={point.Time:HH:mm:ss}");
        }
    }
}


public static class GpxReader
{
    private static readonly XNamespace Ns = "http://www.topografix.com/GPX/1/1";

    public static Gpx Load(string path)
    {
        var root = XDocument.Load(path).Root!;
        return new Gpx
        {
            Creator = root.Attribute("creator")?.Value,
            Tracks  = root.Elements(Ns + "trk").Select(ParseTrack).ToList(),
        };
    }

    private static Track ParseTrack(XElement trk) => new()
    {
        Name = trk.Element(Ns + "name")?.Value.ToString(),
        TrackSegments = trk.Elements(Ns + "trkseg").Select(ParseTrackSegment).ToList(),
    };

    private static TrackSegment ParseTrackSegment(XElement trkseg) => new()
    {
        Points = trkseg.Elements(Ns + "trkpt").Select(ParsePoint).ToList(),
    };

    private static TrackPoint ParsePoint(XElement trkpt) => new()
    {
        // lat/lon jsou atributy a jsou povinné → ! (bez nich bod nemá smysl)
        Latitude = double.Parse(trkpt.Attribute("lat")!.Value, CultureInfo.InvariantCulture),
        Longitude = double.Parse(trkpt.Attribute("lon")!.Value, CultureInfo.InvariantCulture),

        // ele/time jsou ELEMENTY a jsou nepovinné → parsuj jen když existují
        Elevation = trkpt.Element(Ns + "ele")?.Value is { } ele
            ? double.Parse(ele, CultureInfo.InvariantCulture)
            : null,
        Time = trkpt.Element(Ns + "time")?.Value is { } time
            ? DateTimeOffset.Parse(time, CultureInfo.InvariantCulture)
            : null,
    };

}





public record Gpx
{
    public string? Creator { get; init; }
    public IReadOnlyList<Track> Tracks {get; init;} = [];
}

public record Track
{
    public string? Name {get; init;}
    public IReadOnlyList<TrackSegment> TrackSegments {get; init;} = [];
}

public record TrackSegment
{
    public IReadOnlyList<TrackPoint> Points { get; init; } = [];
}

public record TrackPoint
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public double? Elevation { get; init; }
    public DateTimeOffset? Time { get; init; }
}