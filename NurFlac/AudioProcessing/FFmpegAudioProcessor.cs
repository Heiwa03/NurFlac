using NurFlac.AudioProcessing.Interfaces;
using NurFlac.AudioProcessing.SpectralAnalysis.Models;

namespace NurFlac.AudioProcessing;

public class FFmpegAudioProcessor : IAudioProcessor
{
    private readonly SpectralAnalyzerFactory _analyzerFactory;

    public FFmpegAudioProcessor(SpectralAnalyzerFactory analyzerFactory)
    {
        _analyzerFactory = analyzerFactory;
    }

    public async Task<SpectralAnalysisResult> AnalyzeLosslessQualityAsync(string filePath)
    {
        var analyzer = _analyzerFactory.Create(filePath);
        return await analyzer.AnalyzeTrueLosslessAsync(filePath);
    }

    public async Task<bool> VerifyLosslessQualityAsync(string filePath)
    {
        var result = await AnalyzeLosslessQualityAsync(filePath);
        return result.IsTrueLossless;
    }

    public Task<string> ConvertToFlacAsync(string inputPath)
    {
        var outputPath = Path.ChangeExtension(inputPath, ".flac");
        return Task.FromResult(outputPath);
    }
}
