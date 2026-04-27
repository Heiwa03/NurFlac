using NurFlac.AudioProcessing;

namespace NurFlac.Tests;

public class AudioFormatRegistryTests
{
    private readonly AudioFormatRegistry _registry = new();

    // ── GetByExtension ──────────────────────────────────────────

    [Theory]
    [InlineData(".flac", true, "FLAC")]
    [InlineData(".wav",  true, "WAV")]
    [InlineData(".alac", true, "ALAC")]
    [InlineData(".m4a",  true, "ALAC")]
    [InlineData(".aiff", true, "AIFF")]
    [InlineData(".aif",  true, "AIFF")]
    public void GetByExtension_ReturnsLosslessFormat(string extension, bool expectedLossless, string expectedName)
    {
        var fmt = _registry.GetByExtension(extension);

        Assert.NotNull(fmt);
        Assert.Equal(expectedLossless, fmt.IsLossless);
        Assert.Equal(expectedName, fmt.DisplayName);
    }

    [Theory]
    [InlineData(".mp3",  false, "MP3")]
    [InlineData(".aac",  false, "AAC")]
    [InlineData(".ogg",  false, "OGG Vorbis")]
    [InlineData(".opus", false, "Opus")]
    public void GetByExtension_ReturnsLossyFormat(string extension, bool expectedLossless, string expectedName)
    {
        var fmt = _registry.GetByExtension(extension);

        Assert.NotNull(fmt);
        Assert.Equal(expectedLossless, fmt.IsLossless);
        Assert.Equal(expectedName, fmt.DisplayName);
    }

    [Theory]
    [InlineData(".FLAC")]
    [InlineData(".WAV")]
    [InlineData(".MP3")]
    public void GetByExtension_IsCaseInsensitive(string extension)
    {
        var fmt = _registry.GetByExtension(extension);
        Assert.NotNull(fmt);
    }

    [Theory]
    [InlineData(".wma")]
    [InlineData(".ape")]
    [InlineData(".xyz")]
    [InlineData("")]
    public void GetByExtension_ReturnsNull_ForUnknownExtension(string extension)
    {
        var fmt = _registry.GetByExtension(extension);
        Assert.Null(fmt);
    }

    // ── GetByMimeType ───────────────────────────────────────────

    [Theory]
    [InlineData("audio/flac",   true)]
    [InlineData("audio/x-flac", true)]
    [InlineData("audio/wav",    true)]
    [InlineData("audio/x-wav",  true)]
    [InlineData("audio/aiff",   true)]
    [InlineData("audio/alac",   true)]
    public void GetByMimeType_ReturnsLosslessFormat(string mimeType, bool expectedLossless)
    {
        var fmt = _registry.GetByMimeType(mimeType);

        Assert.NotNull(fmt);
        Assert.Equal(expectedLossless, fmt.IsLossless);
    }

    [Theory]
    [InlineData("audio/mpeg",  false)]
    [InlineData("audio/mp3",   false)]
    [InlineData("audio/aac",   false)]
    [InlineData("audio/ogg",   false)]
    [InlineData("audio/opus",  false)]
    public void GetByMimeType_ReturnsLossyFormat(string mimeType, bool expectedLossless)
    {
        var fmt = _registry.GetByMimeType(mimeType);

        Assert.NotNull(fmt);
        Assert.Equal(expectedLossless, fmt.IsLossless);
    }

    [Theory]
    [InlineData("application/octet-stream")]
    [InlineData("video/mp4")]
    [InlineData("text/plain")]
    public void GetByMimeType_ReturnsNull_ForUnknownMimeType(string mimeType)
    {
        var fmt = _registry.GetByMimeType(mimeType);
        Assert.Null(fmt);
    }

    // ── Flyweight: same instance returned ──────────────────────

    [Fact]
    public void GetByExtension_ReturnsSameInstance_ForSameFormat()
    {
        var first  = _registry.GetByExtension(".flac");
        var second = _registry.GetByExtension(".flac");

        Assert.Same(first, second);
    }

    [Fact]
    public void GetByExtension_ReturnsSameInstance_AcrossAliases()
    {
        // .alac and .m4a both map to the ALAC flyweight
        var fromAlac = _registry.GetByExtension(".alac");
        var fromM4a  = _registry.GetByExtension(".m4a");

        Assert.Same(fromAlac, fromM4a);
    }

    [Fact]
    public void GetByExtension_AndGetByMimeType_ReturnSameInstance()
    {
        var byExt  = _registry.GetByExtension(".flac");
        var byMime = _registry.GetByMimeType("audio/flac");

        Assert.Same(byExt, byMime);
    }

    // ── Collection queries ──────────────────────────────────────

    [Fact]
    public void GetAllLossless_ReturnsOnlyLosslessFormats()
    {
        var lossless = _registry.GetAllLossless();

        Assert.All(lossless, f => Assert.True(f.IsLossless));
        Assert.NotEmpty(lossless);
    }

    [Fact]
    public void GetAllLossless_IncludesFlacWavAlacAiff()
    {
        var ids = _registry.GetAllLossless().Select(f => f.Id).ToHashSet();

        Assert.Contains("flac", ids);
        Assert.Contains("wav",  ids);
        Assert.Contains("alac", ids);
        Assert.Contains("aiff", ids);
    }

    [Fact]
    public void GetAll_IncludesBothLosslessAndLossy()
    {
        var all = _registry.GetAll();

        Assert.Contains(all, f => f.IsLossless);
        Assert.Contains(all, f => !f.IsLossless);
    }
}
