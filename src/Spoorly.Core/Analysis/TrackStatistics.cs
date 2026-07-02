namespace Spoorly.Core.Analysis;

public sealed record TrackStatistics
{
    public double Distance { get; init; }        // metry
    public double ElevationGain { get; init; }   // nastoupáno, metry
    public double ElevationLoss { get; init; }   // naklesáno, metry
    public double? MinElevation { get; init; }
    public double? MaxElevation { get; init; }
    public TimeSpan? Duration { get; init; }
    public int PointCount { get; init; }

    // Odvozené hodnoty počítej jako property nad daty výše.
    public double? AverageSpeed =>               // m/s
        Duration is { TotalSeconds: > 0 } d ? Distance / d.TotalSeconds : null;

    public static readonly TrackStatistics Empty = new();
}
