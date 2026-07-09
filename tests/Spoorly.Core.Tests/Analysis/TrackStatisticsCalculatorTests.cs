using Spoorly.Core.Analysis;
using Spoorly.Core.Model;

namespace Spoorly.Core.Tests.Analysis;

public class TrackStatisticsCalculatorTests
{
    // Malý pomocník na výrobu bodů — ať testy nejsou zaneřáděné konstruktory.
    // Elevation i Time jsou volitelné; když je nevyplníš, zůstanou null.
    private static TrackPoint Pt(double lat = 0, double lon = 0, double? ele = null, DateTimeOffset? time = null)
        => new() { Latitude = lat, Longitude = lon, Elevation = ele, Time = time };

    private static TrackSegment Seg(params TrackPoint[] points)
        => new() { Points = points };

    // Stub vzdálenosti: ať dostane jakoukoli dvojici bodů, vždy vrátí 100.
    // Tím oddělíme agregaci (sčítání úseků) od reálného 2D vzorce —
    // stejný trik jako u Distance.Slope, kde jsme podstrčili (a, b) => 40.
    private static readonly Func<TrackPoint, TrackPoint, double> Fixed100 = (_, _) => 100.0;

    [Fact]
    public void Compute_EmptySegment_ReturnsEmpty()
    {
        var segment = Seg();

        var result = TrackStatisticsCalculator.Compute(segment);

        // TrackStatistics je record → porovnává se podle hodnot, ne referencí.
        Assert.Equal(TrackStatistics.Empty, result);
    }

    [Fact]
    public void Compute_ThreePoints_SumsDistanceOverAdjacentPairs()
    {
        // 3 body = 2 sousední úseky. Se stubem 100 na úsek čekáme 200.
        var segment = Seg(Pt(), Pt(), Pt());

        var result = TrackStatisticsCalculator.Compute(segment, Fixed100);

        Assert.Equal(200.0, result.Distance, tolerance: 0);
        Assert.Equal(3, result.PointCount);
    }

    [Fact]
    public void Compute_Elevations_AccumulatesGainAndLoss()
    {
        // Výšky 100 → 150 → 120: nastoupáno 50, naklesáno 30.
        // Pozor: počítají se ROZDÍLY mezi sousedy, ne součet výšek.
        var segment = Seg(
            Pt(ele: 100),
            Pt(ele: 150),
            Pt(ele: 120));

        var result = TrackStatisticsCalculator.Compute(segment, Fixed100);

        Assert.Equal(50.0, result.ElevationGain, tolerance: 0);
        Assert.Equal(30.0, result.ElevationLoss, tolerance: 0);
        Assert.Equal(100.0, result.MinElevation);
        Assert.Equal(150.0, result.MaxElevation);
    }

    [Fact]
    public void Compute_MissingElevation_SkipsElevationButKeepsDistanceAndCount()
    {
        // Prostřední bod nemá výšku. Gain/Loss/Min/Max ho přeskočí
        // (potřebují výšku u OBOU sousedů), ale vzdálenost a počet bodů běží dál.
        var segment = Seg(
            Pt(ele: 100),
            Pt(ele: null),
            Pt(ele: 120));

        var result = TrackStatisticsCalculator.Compute(segment, Fixed100);

        Assert.Equal(0.0, result.ElevationGain, tolerance: 0);
        Assert.Equal(0.0, result.ElevationLoss, tolerance: 0);
        Assert.Equal(100.0, result.MinElevation);   // jen z bodů, co výšku mají
        Assert.Equal(120.0, result.MaxElevation);
        Assert.Equal(200.0, result.Distance, tolerance: 0);
        Assert.Equal(3, result.PointCount);
    }

    [Fact]
    public void Compute_WithTimes_ComputesDuration()
    {
        var start = new DateTimeOffset(2026, 7, 8, 10, 0, 0, TimeSpan.Zero);
        var segment = Seg(
            Pt(time: start),
            Pt(time: start.AddMinutes(90)));

        var result = TrackStatisticsCalculator.Compute(segment, Fixed100);

        Assert.Equal(TimeSpan.FromMinutes(90), result.Duration);
    }

    [Fact]
    public void Compute_WithoutTimes_DurationIsNull()
    {
        var segment = Seg(Pt(), Pt());

        var result = TrackStatisticsCalculator.Compute(segment, Fixed100);

        Assert.Null(result.Duration);
    }

    [Fact]
    public void Compute_MultipleSegments_DoesNotCountDistanceBetweenSegments()
    {
        // Doménové rozhodnutí (viz komentář v TrackStatisticsCalculator.Merge):
        // mezera mezi koncem jednoho a začátkem druhého segmentu = pauza/výpadek GPS,
        // NEpočítá se do vzdálenosti. 2 segmenty × 1 úsek à 100 = 200, ne 300.
        var track = new Track
        {
            TrackSegments =
            [
                Seg(Pt(), Pt()),
                Seg(Pt(), Pt()),
            ],
        };

        var result = TrackStatisticsCalculator.Compute(track, Fixed100);

        Assert.Equal(200.0, result.Distance, tolerance: 0);
        Assert.Equal(4, result.PointCount);
    }
}
