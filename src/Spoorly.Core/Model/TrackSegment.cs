namespace Spoorly.Core.Model;

public record TrackSegment
{
    public IReadOnlyList<TrackPoint> Points { get; init; } = [];
}
