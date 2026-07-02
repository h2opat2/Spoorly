using Spoorly.Core.Geo;
using Spoorly.Core.Model;

namespace Spoorly.Core.Analysis;

public static class TrackStatisticsCalculator
{
    public static TrackStatistics Compute(
        TrackSegment segment,
        Func<TrackPoint, TrackPoint, double>? pointDistance = null)
    {
        var distance = pointDistance ?? Distance.Equirectangular;
        var points = segment.Points;
        if (points.Count == 0)
            return TrackStatistics.Empty;

        var total = 0.0;
        var gain = 0.0;
        var loss = 0.0;
        double? min = points[0].Elevation;
        double? max = points[0].Elevation;

        for (var i = 1; i < points.Count; i++)
        {
            var prev = points[i - 1];
            var curr = points[i];
            total += distance(prev, curr);

            if (prev.Elevation is { } e1 && curr.Elevation is { } e2)
            {
                var delta = e2 - e1;
                if (delta > 0) gain += delta; else loss -= delta;
            }

            if (curr.Elevation is { } ele)
            {
                min = min is { } m ? Math.Min(m, ele) : ele;
                max = max is { } x ? Math.Max(x, ele) : ele;
            }
        }

        var duration = points[0].Time is { } start && points[^1].Time is { } end
            ? end - start
            : (TimeSpan?)null;

        return new TrackStatistics
        {
            Distance = total,
            ElevationGain = gain,
            ElevationLoss = loss,
            MinElevation = min,
            MaxElevation = max,
            Duration = duration,
            PointCount = points.Count,
        };
    }

    public static TrackStatistics Compute(
        Track track,
        Func<TrackPoint, TrackPoint, double>? pointDistance = null)
        => track.TrackSegments
            .Select(s => Compute(s, pointDistance))
            .Aggregate(TrackStatistics.Empty, Merge);

    public static TrackStatistics Compute(
        Gpx gpx,
        Func<TrackPoint, TrackPoint, double>? pointDistance = null)
        => gpx.Tracks
            .Select(t => Compute(t, pointDistance))
            .Aggregate(TrackStatistics.Empty, Merge);

    // Sloučení dvou snímků — proto počítám segmenty zvlášť a sčítám je:
    // mezi koncem jednoho a začátkem dalšího segmentu se vzdálenost NEpočítá
    // (to je typicky pauza/výpadek GPS, ne skutečně ujetý úsek).
    private static TrackStatistics Merge(TrackStatistics a, TrackStatistics b) => new()
    {
        Distance = a.Distance + b.Distance,
        ElevationGain = a.ElevationGain + b.ElevationGain,
        ElevationLoss = a.ElevationLoss + b.ElevationLoss,
        MinElevation = Least(a.MinElevation, b.MinElevation),
        MaxElevation = Most(a.MaxElevation, b.MaxElevation),
        Duration = a.Duration + b.Duration ?? a.Duration ?? b.Duration,
        PointCount = a.PointCount + b.PointCount,
    };

    private static double? Least(double? a, double? b)
        => a is null ? b : b is null ? a : Math.Min(a.Value, b.Value);

    private static double? Most(double? a, double? b)
        => a is null ? b : b is null ? a : Math.Max(a.Value, b.Value);
}
