using Spoorly.Core.Geo;
using Spoorly.Core.Model;

namespace Spoorly.Core.Analysis;

public static class TrackProfileBuilder
{
    public static TrackProfile Build(
        Track track,
        Func<TrackPoint, TrackPoint, double>? pointDistance = null)
    {
        var distance = pointDistance ?? Distance.Equirectangular;
        var samples = new List<ProfilePoint>();
        var cumulative = 0.0;

        foreach (var segment in track.TrackSegments)
        {
            // Nový segment: vzdálenost od konce předchozího segmentu nepřičítáme
            // (pauza/výpadek), ale kumulativní osa plynule pokračuje dál.
            TrackPoint? previous = null;

            foreach (var point in segment.Points)
            {
                if (previous is { } prev)
                    cumulative += distance(prev, point);

                samples.Add(new ProfilePoint { Distance = cumulative, Point = point });
                previous = point;
            }
        }

        return samples.Count == 0 ? TrackProfile.Empty : new TrackProfile { Points = samples };
    }
}
