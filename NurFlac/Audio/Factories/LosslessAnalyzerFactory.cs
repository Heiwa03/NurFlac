// ConcreteFactory 1 — creates analyzers tuned for true lossless verification.
using NurFlac.Audio.Abstractions;
using NurFlac.Audio.Analyzers;

namespace NurFlac.Audio.Factories;

public sealed class LosslessAnalyzerFactory(IFfmpegTool ffmpegTool) : IAudioAnalyzerFactory
{
    public string Category => "Lossless";

    public ISpectralAnalyzer CreateFlacAnalyzer() => new FlacSpectralAnalyzer(ffmpegTool);
    public ISpectralAnalyzer CreateWavAnalyzer()  => new WavSpectralAnalyzer(ffmpegTool);
    public ISpectralAnalyzer CreateAlacAnalyzer() => new AlacSpectralAnalyzer(ffmpegTool);
    public ISpectralAnalyzer CreateAiffAnalyzer() => new AiffSpectralAnalyzer(ffmpegTool);

    public ISpectralAnalyzer? CreateForExtension(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".flac"        => CreateFlacAnalyzer(),
            ".wav"         => CreateWavAnalyzer(),
            ".m4a" or ".alac" => CreateAlacAnalyzer(),
            ".aiff" or ".aif" => CreateAiffAnalyzer(),
            _              => null
        };
}
