
namespace Spoorly.Core.Io;

public static class ActivityParserFactory
{
    public static IActivityParser ForFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();

        return ext switch
        {
            ".gpx" => new GpxReader(),
            _ => throw new NotSupportedException($"Nepodporovaný formát: {ext}")
        };
    }
}