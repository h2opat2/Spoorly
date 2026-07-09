using Spoorly.Core.Io;

namespace Spoorly.Core.Tests.Io;

public class ActivityParserFactoryTests
{
    [Fact]
    public void ForFile_GpxExtension_ReturnsGpxReader()
    {
        var parser = ActivityParserFactory.ForFile("trasa.gpx");

        // Factory vrací IActivityParser (abstrakci) — testem ověříme,
        // že za ním pro .gpx opravdu stojí GpxReader.
        Assert.IsType<GpxReader>(parser);
    }

    [Fact]
    public void ForFile_UppercaseExtension_IsCaseInsensitive()
    {
        // Přípona se normalizuje přes ToLowerInvariant → .GPX musí projít stejně.
        var parser = ActivityParserFactory.ForFile("TRASA.GPX");

        Assert.IsType<GpxReader>(parser);
    }

    [Fact]
    public void ForFile_UnknownExtension_ThrowsNotSupported()
    {
        // Neznámý formát = platný požadavek, jen ho neumíme → NotSupportedException.
        var ex = Assert.Throws<NotSupportedException>(
            () => ActivityParserFactory.ForFile("trasa.tcx"));

        // Zpráva má obsahovat příponu, ať je chyba čitelná.
        Assert.Contains(".tcx", ex.Message);
    }
}
