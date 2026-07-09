using System.Globalization;
using System.Xml.Linq;
using Spoorly.Core.Model;

namespace Spoorly.Core.Io;

public class GpxReader : IActivityParser
{
    private static readonly XNamespace Ns = "http://www.topografix.com/GPX/1/1";

    public Activity Parse(Stream stream)
    {
        var root = XDocument.Load(stream).Root!;
        return new Activity
        {
            Creator = root.Attribute("creator")?.Value,
            Tracks = root.Elements(Ns + "trk").Select(ParseTrack).ToList(),
        };
    }

    private static Track ParseTrack(XElement trk) => new()
    {
        Name = trk.Element(Ns + "name")?.Value,
        TrackSegments = trk.Elements(Ns + "trkseg").Select(ParseTrackSegment).ToList(),
    };

    private static TrackSegment ParseTrackSegment(XElement trkseg) => new()
    {
        Points = trkseg.Elements(Ns + "trkpt").Select(ParsePoint).ToList(),
    };

    private static TrackPoint ParsePoint(XElement trkpt) => new()
    {
        // lat/lon jsou atributy a jsou povinné → ! (bez nich bod nemá smysl)
        Latitude = double.Parse(trkpt.Attribute("lat")!.Value, CultureInfo.InvariantCulture),
        Longitude = double.Parse(trkpt.Attribute("lon")!.Value, CultureInfo.InvariantCulture),

        // ele/time jsou ELEMENTY a jsou nepovinné → parsuj jen když existují
        Elevation = trkpt.Element(Ns + "ele")?.Value is { } ele
            ? double.Parse(ele, CultureInfo.InvariantCulture)
            : null,
        Time = trkpt.Element(Ns + "time")?.Value is { } time
            ? DateTimeOffset.Parse(time, CultureInfo.InvariantCulture)
            : null,
    };
}
