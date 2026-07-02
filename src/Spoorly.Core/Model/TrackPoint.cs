namespace Spoorly.Core.Model;

public record TrackPoint
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public double? Elevation { get; init; }
    public DateTimeOffset? Time { get; init; }
}
