using System.Diagnostics;
using Spoorly.Core.Analysis;
using Spoorly.Core.Geo;
using Spoorly.Core.Io;
using Spoorly.Core.Model;

// --- malý soubor: dvojí měření (studený vs. teplý běh) ---
Spoorly.Core.Model.Activity smallGpx = Load("../../data/test1.gpx", "1. načtení (studené)");
Load("../../data/test1.gpx", "2. načtení (teplé)");
PrintTracks(smallGpx);

// --- velký, reálný soubor ---
Spoorly.Core.Model.Activity bigGpx = Load("../../data/etapa01_den02.gpx", "Reálný soubor");
Console.WriteLine($"Celkem bodů: {bigGpx.Tracks.Sum(t => t.TrackSegments.Sum(s => s.Points.Count))}");
PrintStats(bigGpx);
PrintProfile(bigGpx.Tracks[0]);

// Načte soubor, změří dobu a vypíše ji. Vrací načtená data.
Spoorly.Core.Model.Activity Load(string path, string label)
{
    var sw = Stopwatch.StartNew();
    //var parser = new GpxReader(); // puvodni volani bez Factory
    IActivityParser parser = ActivityParserFactory.ForFile(path);
    using var stream = File.OpenRead(path);
    var activity = parser.Parse(stream);
    sw.Stop();
    Console.WriteLine($"{label}: {sw.Elapsed.TotalMilliseconds:F3} ms");
    return activity;
}

// Spočítá a vypíše statistiky trasy – rovinnou (2D) i skloněnou (3D) vzdálenost.
static void PrintStats(Spoorly.Core.Model.Activity gpx)
{
    var flat = TrackStatisticsCalculator.Compute(gpx);
    var slope = TrackStatisticsCalculator.Compute(gpx, (a, b) => Distance.Slope(a, b));

    Console.WriteLine($"Délka (2D):   {flat.Distance / 1000:F2} km");
    Console.WriteLine($"Délka (3D):   {slope.Distance / 1000:F2} km");
    Console.WriteLine($"Převýšení:    +{slope.ElevationGain:F0} m / -{slope.ElevationLoss:F0} m");
    Console.WriteLine($"Výška:        {slope.MinElevation:F0} – {slope.MaxElevation:F0} m");
    if (slope.Duration is { } duration)
        Console.WriteLine($"Doba:         {duration:hh\\:mm\\:ss}");
    if (slope.AverageSpeed is { } speed)
        Console.WriteLine($"Prům. rychlost: {speed * 3.6:F1} km/h");
}

// Postaví profil trasy a ukáže, že jeho celková délka sedí,
// plus vzorek bodů zhruba po 5 km (osa X pro výškový profil).
static void PrintProfile(Track track)
{
    var profile = TrackProfileBuilder.Build(track);
    Console.WriteLine($"Profil: {profile.Points.Count} bodů, délka {profile.TotalDistance / 1000:F2} km");

    var nextMark = 0.0;
    foreach (var sample in profile.Points)
    {
        if (sample.Distance < nextMark) continue;
        Console.WriteLine($"  {sample.Distance / 1000,5:F1} km  ele={sample.Point.Elevation:F0} m");
        nextMark += 1000;
    }
}

static void PrintTracks(Spoorly.Core.Model.Activity activity)
{
    Console.WriteLine($"Creator: {activity.Creator}");
    foreach (var track in activity.Tracks)
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
