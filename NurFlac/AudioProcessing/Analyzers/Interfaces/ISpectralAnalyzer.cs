using NurFlac.AudioProcessing.SpectralAnalysis.Models;

namespace NurFlac.AudioProcessing.Analyzers.Interfaces;

public interface ISpectralAnalyzer
{
    Task<double> GetFrequencyCutoffAsync(string filePath);
    Task<SpectralAnalysisResult> AnalyzeTrueLosslessAsync(string filePath);
    
    // Legacy support to avoid breaking existing code
    [Obsolete("Use AnalyzeTrueLosslessAsync")]
    Task<bool> IsTrueLosslessAsync(string filePath);
}
