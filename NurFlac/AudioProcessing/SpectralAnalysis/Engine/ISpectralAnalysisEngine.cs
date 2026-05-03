using NurFlac.AudioProcessing.SpectralAnalysis.Models;

namespace NurFlac.AudioProcessing.SpectralAnalysis.Engine;

public interface ISpectralAnalysisEngine
{
    Task<SpectralAnalysisResult> AnalyzeAsync(float[] samples, int sampleRate, ScanConfig config);
}
