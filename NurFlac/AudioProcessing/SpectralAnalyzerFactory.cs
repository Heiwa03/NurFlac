using NurFlac.AudioProcessing.Analyzers;
using NurFlac.AudioProcessing.Analyzers.Interfaces;
using System.IO;

namespace NurFlac.AudioProcessing;

/// <summary>
/// Factory Method pattern — creates the correct <see cref="ISpectralAnalyzer"/>
/// based on the audio file extension. The caller never needs to know the concrete type.
/// </summary>
public class SpectralAnalyzerFactory
{
    /// <summary>
    /// Factory method that returns the appropriate analyzer for the given file.
    /// </summary>
    /// <param name="filePath">Path to the audio file.</param>
    /// <returns>A format-specific <see cref="ISpectralAnalyzer"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown when the audio format is unsupported.</exception>
    public ISpectralAnalyzer Create(string filePath)
    {
        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();

        return extension switch
        {
            ".flac" => new FlacSpectralAnalyzer(),
            ".wav"  => new WavSpectralAnalyzer(),
            ".alac" or ".m4a" => new AlacSpectralAnalyzer(),
            ".aiff" or ".aif" => new AiffSpectralAnalyzer(),
            _ => throw new NotSupportedException(
                     $"Audio format '{extension}' is not supported for spectral analysis.")
        };
    }
}
