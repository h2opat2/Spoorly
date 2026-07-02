namespace Spoorly.Core.Model;

public record Track
{
    public string? Name { get; init; }
    public IReadOnlyList<TrackSegment> TrackSegments { get; init; } = [];
}
