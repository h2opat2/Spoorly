using Spoorly.Core.Geo;
using Spoorly.Core.Model;

namespace Spoorly.Core.Tests.Geo;

public class DistanceTests
{
    [Fact]
    public void Equirectangular_SamePoint_ReturnsZero()
    {
        var point = new TrackPoint
        {
            Longitude = 15.0,
            Latitude = 30.2
        };

        var distance = Distance.Equirectangular(point, point);

        Assert.Equal(0, distance, precision: 6);
    }

    [Theory]
    [InlineData(50.2010, 30.1000, 50.2011, 30.1001, 14.7)]
    [InlineData(72, 20, 72, 20.0003, 33.4)]
    public void Equirectangular_VariousPoints_ReturnsDistance(
        double lon1,
        double lat1,
        double lon2,
        double lat2,
        double expected)
    {
        var point1 = new TrackPoint
        {
            Longitude = lon1,
            Latitude = lat1
        };
        var point2 = new TrackPoint
        {
            Longitude = lon2,
            Latitude = lat2
        };

        var result = Distance.Equirectangular(point1, point2);
        Assert.Equal(expected, result, precision: 1);
    }

    [Fact]
    public void Slope_Pythagoras_ReturnSlantDistance()
    {
        var point1 = new TrackPoint
        {
            Longitude = 10,
            Latitude = 10,
            Elevation = 0
        };
        var point2 = new TrackPoint
        {
            Longitude = 10,
            Latitude = 10,
            Elevation = 30
        };

        var result = Distance.Slope(point1, point2, (a, b) => 40.0);

        Assert.Equal(50.0, result, tolerance: 0);
    }

    [Fact]
    public void Slope_NullElevation_ReturnHorizontalDistance()
    {
        var point1 = new TrackPoint
        {
            Longitude = 10,
            Latitude = 10,
            Elevation = 0
        };
        var point2 = new TrackPoint
        {
            Longitude = 10,
            Latitude = 10
        };

        var result = Distance.Slope(point1, point2, (a, b) => 40.0);

        Assert.Equal(40.0, result, tolerance: 0);
    }

    [Fact]
    public void Slope_NullElevationBoth_ReturnHorizontalDistance()
    {
        var point1 = new TrackPoint
        {
            Longitude = 10,
            Latitude = 10
        };
        var point2 = new TrackPoint
        {
            Longitude = 10,
            Latitude = 10
        };

        var result = Distance.Slope(point1, point2, (a, b) => 40.0);

        Assert.Equal(40.0, result, tolerance: 0);
    }
}


