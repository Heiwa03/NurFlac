using NurFlac.Audio.Models;

namespace NurFlac.Audio.Abstractions;

public interface ISpectralAnalyzer
{
    Task<SpectralAnalysisResult> AnalyzeAsync(string filePath, CancellationToken ct = default);
}
