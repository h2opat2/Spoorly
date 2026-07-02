using Spoorly.Core.Model;

namespace Spoorly.Core.Geo;

public static class Distance
{
    private const double EarthRadiusMeters = 6_371_000;

    /// <summary>
    /// Vzdálenost dvou bodů v metrech pomocí equirektangulární aproximace.
    /// Rychlá a pro krátké úseky (sousední body trasy) dostatečně přesná.
    /// </summary>
    public static double Equirectangular(TrackPoint p1, TrackPoint p2)
    {
        var lat1 = DegreesToRadians(p1.Latitude);
        var lat2 = DegreesToRadians(p2.Latitude);
        var dLat = lat2 - lat1;
        var dLon = DegreesToRadians(p2.Longitude - p1.Longitude);

        var x = dLon * Math.Cos((lat1 + lat2) / 2);
        var y = dLat;

        return Math.Sqrt(x * x + y * y) * EarthRadiusMeters;
    }

    /// <summary>
    /// Skloněná (3D) vzdálenost dvou bodů v metrech – vodorovná vzdálenost
    /// doplněná o převýšení. Pokud u některého bodu chybí výška, vrací se
    /// jen vodorovná vzdálenost.
    /// </summary>
    /// <param name="horizontalDistance">
    /// Funkce počítající vodorovnou vzdálenost dvou bodů v metrech.
    /// Když se neuvede, použije se <see cref="Equirectangular"/>.
    /// </param>
    public static double Slope(
        TrackPoint p1,
        TrackPoint p2,
        Func<TrackPoint, TrackPoint, double>? horizontalDistance = null)
    {
        // Výchozí hodnotu parametru nelze nastavit na metodu (musí být konstanta),
        // proto ji doplníme tady.
        var distance = horizontalDistance ?? Equirectangular;
        var horizontal = distance(p1, p2);

        // Převýšení umíme spočítat jen když mají oba body výšku.
        if (p1.Elevation is not { } ele1 || p2.Elevation is not { } ele2)
            return horizontal;

        var dElevation = ele2 - ele1;
        return Math.Sqrt(horizontal * horizontal + dElevation * dElevation);
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}
