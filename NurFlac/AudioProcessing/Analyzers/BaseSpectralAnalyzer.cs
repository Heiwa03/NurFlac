using NurFlac.AudioProcessing.Analyzers.Interfaces;

namespace NurFlac.AudioProcessing.Analyzers;

/// <summary>
/// Template base class for spectral analyzers.
/// Subclasses provide format-specific decoding; the cutoff logic is shared.
/// </summary>
public abstract class BaseSpectralAnalyzer : ISpectralAnalyzer
{
    // Known cutoff thresholds (Hz) for common lossy codecs
    private const double Mp3CutoffThreshold = 16_000;
    private const double AacCutoffThreshold = 20_000;
    private const double LosslessMinCutoff = 22_000;

    public async Task<double> GetFrequencyCutoffAsync(string filePath)
    {
        var samples = await DecodeToPcmAsync(filePath);
        return AnalyzeSpectrum(samples);
    }

    public async Task<bool> IsTrueLosslessAsync(string filePath)
    {
        var cutoff = await GetFrequencyCutoffAsync(filePath);
        return cutoff >= LosslessMinCutoff;
    }

    /// <summary>
    /// Format-specific decoding — each subclass knows how to produce raw PCM samples.
    /// </summary>
    protected abstract Task<float[]> DecodeToPcmAsync(string filePath);

    /// <summary>
    /// Shared spectral analysis logic (placeholder — real impl would use FFT).
    /// </summary>
    private static double AnalyzeSpectrum(float[] samples)
    {
        // TODO: Replace with real FFT-based analysis.
        // For now, return a placeholder indicating full-range audio.
        return samples.Length > 0 ? 22_050 : 0;
    }
}