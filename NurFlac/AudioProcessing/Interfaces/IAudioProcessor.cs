using NurFlac.AudioProcessing.SpectralAnalysis.Models;

namespace NurFlac.AudioProcessing.Interfaces;

public interface IAudioProcessor
{
    Task<SpectralAnalysisResult> AnalyzeLosslessQualityAsync(string filePath);
    Task<string> ConvertToFlacAsync(string inputPath);
    
    // Legacy support
    [Obsolete("Use AnalyzeLosslessQualityAsync")]
    Task<bool> VerifyLosslessQualityAsync(string filePath);
}
