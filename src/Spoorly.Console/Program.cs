using System.Diagnostics;
using Spoorly.Core.Io;
using Spoorly.Core.Model;

// --- malý soubor: dvojí měření (studený vs. teplý běh) ---
Gpx small = Load("../../data/test1.gpx", "1. načtení (studené)");
Load("../../data/test1.gpx", "2. načtení (teplé)");
PrintTracks(small);

// --- velký, reálný soubor ---
Gpx big = Load("../../data/etapa01_den02.gpx", "Reálný soubor");
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
