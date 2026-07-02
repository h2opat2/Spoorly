using Spoorly.Core.Model;

namespace Spoorly.Core.Analysis;

// Jeden vzorek profilu: bod trasy spolu s jeho vzdáleností od startu.
public readonly record struct ProfilePoint
{
    public required double Distance { get; init; }  // metry od startu
    public required TrackPoint Point { get; init; }
}

// Trasa „rozvinutá" podél vzdálenostní osy – měřená polylinie (M-hodnoty).
// Osa X (Distance) roste monotónně; z ní se kreslí výškový profil,
// počítají splity po kilometru nebo tempo v čase.
public sealed record TrackProfile
{
    public IReadOnlyList<ProfilePoint> Points { get; init; } = [];

    public double TotalDistance => Points.Count == 0 ? 0 : Points[^1].Distance;

    public static readonly TrackProfile Empty = new();
}
