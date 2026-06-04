// ConcreteFactory 2 — creates analyzers in "detection mode" for potentially
// lossy files or files that need more aggressive cutoff verification.
using NurFlac.Audio.Abstractions;
using NurFlac.Audio.Analyzers;

namespace NurFlac.Audio.Factories;

public sealed class LossyDetectorFactory(IFfmpegTool ffmpegTool) : IAudioAnalyzerFactory
{
    public string Category => "LossyDetector";

    // All products still use spectral analysis; the factory distinction
    // allows future injection of detection-mode ScanConfig overrides.
    public ISpectralAnalyzer CreateFlacAnalyzer() => new FlacSpectralAnalyzer(ffmpegTool);
    public ISpectralAnalyzer CreateWavAnalyzer()  => new WavSpectralAnalyzer(ffmpegTool);
    public ISpectralAnalyzer CreateAlacAnalyzer() => new AlacSpectralAnalyzer(ffmpegTool);
    public ISpectralAnalyzer CreateAiffAnalyzer() => new AiffSpectralAnalyzer(ffmpegTool);

    public ISpectralAnalyzer? CreateForExtension(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".flac"           => CreateFlacAnalyzer(),
            ".wav"            => CreateWavAnalyzer(),
            ".m4a" or ".alac" => CreateAlacAnalyzer(),
            ".aiff" or ".aif" => CreateAiffAnalyzer(),
            _                 => null
        };
}
