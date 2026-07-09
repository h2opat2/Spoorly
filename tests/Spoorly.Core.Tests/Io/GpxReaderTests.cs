using System.Text;
using Spoorly.Core.Io;

namespace Spoorly.Core.Tests.Io;

public class GpxReaderTests
{
    // Minimální validní GPX 1.1 se dvěma body — jeden s výškou i časem,
    // druhý bez času (ať test pokryje i nepovinné elementy).
    private const string SampleGpx = """
        <?xml version="1.0" encoding="UTF-8"?>
        <gpx creator="Spoorly.Tests" xmlns="http://www.topografix.com/GPX/1/1">
          <trk>
            <name>Testovací trasa</name>
            <trkseg>
              <trkpt lat="50.1" lon="14.4">
                <ele>200</ele>
                <time>2026-07-08T06:00:00Z</time>
              </trkpt>
              <trkpt lat="50.2" lon="14.5">
                <ele>250</ele>
              </trkpt>
            </trkseg>
          </trk>
        </gpx>
        """;

    // GPX z paměti místo z disku — přesně proto rozhraní bere Stream:
    // test je rychlý, deterministický a nepotřebuje fixture soubor.
    private static Stream StreamFrom(string content)
        => new MemoryStream(Encoding.UTF8.GetBytes(content));

    [Fact]
    public void Parse_ValidGpx_ReadsCreatorTrackAndPoints()
    {
        var parser = new GpxReader();

        using var stream = StreamFrom(SampleGpx);
        var activity = parser.Parse(stream);

        Assert.Equal("Spoorly.Tests", activity.Creator);

        var track = Assert.Single(activity.Tracks);
        Assert.Equal("Testovací trasa", track.Name);

        var segment = Assert.Single(track.TrackSegments);
        Assert.Equal(2, segment.Points.Count);
    }

    [Fact]
    public void Parse_FirstPoint_HasCoordinatesElevationAndTime()
    {
        var parser = new GpxReader();

        using var stream = StreamFrom(SampleGpx);
        var activity = parser.Parse(stream);

        var first = activity.Tracks[0].TrackSegments[0].Points[0];
        Assert.Equal(50.1, first.Latitude, precision: 6);
        Assert.Equal(14.4, first.Longitude, precision: 6);
        Assert.Equal(200, first.Elevation);
        Assert.NotNull(first.Time);
    }

    [Fact]
    public void Parse_PointWithoutTime_LeavesTimeNull()
    {
        var parser = new GpxReader();

        using var stream = StreamFrom(SampleGpx);
        var activity = parser.Parse(stream);

        // Druhý bod nemá <time> → nepovinný element zůstane null.
        var second = activity.Tracks[0].TrackSegments[0].Points[1];
        Assert.Null(second.Time);
        Assert.Equal(250, second.Elevation);
    }
}
