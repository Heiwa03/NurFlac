namespace NurFlac.AudioProcessing.Analyzers.Interfaces;

/// <summary>
/// Analyzes the spectral content of an audio file to detect transcodes.
/// </summary>
public interface ISpectralAnalyzer
{
    /// <summary>
    /// Returns the detected frequency cutoff in Hz.
    /// A true lossless file should have energy up to ~22 kHz.
    /// </summary>
    Task<double> GetFrequencyCutoffAsync(string filePath);

    /// <summary>
    /// Returns true if the file appears to be genuinely lossless
    /// (no signs of lossy-to-lossless transcode).
    /// </summary>
    Task<bool> IsTrueLosslessAsync(string filePath);
}