using NurFlac.Audio.Models;

namespace NurFlac.Audio.Engine;

public interface ISpectralAnalysisEngine
{
    Task<SpectralAnalysisResult> AnalyzeAsync(float[] samples, int sampleRate, ScanConfig config);
}
