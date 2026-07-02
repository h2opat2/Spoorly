namespace Spoorly.Core.Model;

public record Gpx
{
    public string? Creator { get; init; }
    public IReadOnlyList<Track> Tracks { get; init; } = [];
}
