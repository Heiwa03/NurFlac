using NurFlac.AudioProcessing;
using NurFlac.AudioProcessing.Analyzers;

namespace NurFlac.Tests;

public class SpectralAnalyzerFactoryTests
{
    private readonly SpectralAnalyzerFactory _factory = new();

    [Theory]
    [InlineData("song.flac", typeof(FlacSpectralAnalyzer))]
    [InlineData("song.wav", typeof(WavSpectralAnalyzer))]
    [InlineData("song.alac", typeof(AlacSpectralAnalyzer))]
    [InlineData("song.m4a", typeof(AlacSpectralAnalyzer))]
    [InlineData("song.aiff", typeof(AiffSpectralAnalyzer))]
    [InlineData("song.aif", typeof(AiffSpectralAnalyzer))]
    public void Create_ReturnsCorrectAnalyzerType(string filePath, Type expectedType)
    {
        var analyzer = _factory.Create(filePath);
        Assert.IsType(expectedType, analyzer);
    }

    [Theory]
    [InlineData("song.mp3")]
    [InlineData("song.ogg")]
    [InlineData("song.wma")]
    [InlineData("song")]
    public void Create_ThrowsForUnsupportedFormat(string filePath)
    {
        Assert.Throws<NotSupportedException>(() => _factory.Create(filePath));
    }

    [Fact]
    public void Create_IsCaseInsensitive()
    {
        var analyzer = _factory.Create("SONG.FLAC");
        Assert.IsType<FlacSpectralAnalyzer>(analyzer);
    }

    [Fact]
    public async Task CreatedAnalyzer_CanPerformAnalysis()
    {
        var analyzer = _factory.Create("test.flac");
        var cutoff = await analyzer.GetFrequencyCutoffAsync("test.flac");
        Assert.True(cutoff > 0);
    }

    [Fact]
    public async Task CreatedAnalyzer_DetectsTrueLossless()
    {
        var analyzer = _factory.Create("test.wav");
        var result = await analyzer.IsTrueLosslessAsync("test.wav");
        Assert.True(result);
    }
}
