using NurFlac.AudioProcessing.Interfaces;

namespace NurFlac.AudioProcessing;

/// <summary>
/// Concrete <see cref="IAudioProcessor"/> that uses FFmpeg for conversion
/// and delegates spectral verification to the <see cref="SpectralAnalyzerFactory"/>.
/// </summary>
public class FFmpegAudioProcessor : IAudioProcessor
{
    private readonly SpectralAnalyzerFactory _analyzerFactory;

    public FFmpegAudioProcessor(SpectralAnalyzerFactory analyzerFactory)
    {
        _analyzerFactory = analyzerFactory;
    }

    public async Task<bool> VerifyLosslessQualityAsync(string filePath)
    {
        var analyzer = _analyzerFactory.Create(filePath);
        return await analyzer.IsTrueLosslessAsync(filePath);
    }

    public Task<string> ConvertToFlacAsync(string inputPath)
    {
        // TODO: Shell out to ffmpeg to convert the input to FLAC.
        var outputPath = Path.ChangeExtension(inputPath, ".flac");
        return Task.FromResult(outputPath);
    }
}
