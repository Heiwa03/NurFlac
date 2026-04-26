using NurFlac.AudioProcessing;

namespace NurFlac.Tests;

public class FFmpegAudioProcessorTests
{
    private readonly FFmpegAudioProcessor _processor = new(new SpectralAnalyzerFactory());

    [Theory]
    [InlineData("song.flac")]
    [InlineData("song.wav")]
    [InlineData("song.aiff")]
    public async Task VerifyLosslessQualityAsync_ReturnsTrueForLosslessFormats(string filePath)
    {
        var result = await _processor.VerifyLosslessQualityAsync(filePath);
        Assert.True(result);
    }

    [Fact]
    public async Task VerifyLosslessQualityAsync_ThrowsForUnsupportedFormat()
    {
        await Assert.ThrowsAsync<NotSupportedException>(
            () => _processor.VerifyLosslessQualityAsync("song.mp3"));
    }

    [Fact]
    public async Task ConvertToFlacAsync_ReturnsFlacPath()
    {
        var result = await _processor.ConvertToFlacAsync("input.wav");
        Assert.EndsWith(".flac", result);
    }
}