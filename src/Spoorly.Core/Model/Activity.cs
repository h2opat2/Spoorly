namespace Spoorly.Core.Model;

public record Activity
{
    public string? Creator { get; init; }
    public IReadOnlyList<Track> Tracks { get; init; } = [];
}
