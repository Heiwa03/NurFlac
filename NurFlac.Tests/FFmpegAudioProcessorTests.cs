using NurFlac.AudioProcessing;

namespace NurFlac.Tests;

public class FFmpegAudioProcessorTests
{
    private readonly FFmpegAudioProcessor _processor = new(new SpectralAnalyzerFactory());

    [Theory]
    [InlineData("test_song.flac")]
    [InlineData("test_song.wav")]
    [InlineData("test_song.aiff")]
    public async Task VerifyLosslessQualityAsync_ReturnsTrueForLosslessFormats(string filePath)
    {
        var result = await _processor.VerifyLosslessQualityAsync(filePath);
        Assert.True(result);
    }
}
