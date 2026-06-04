using NurFlac.Audio.Abstractions;
using NurFlac.Audio.Engine;
using NurFlac.Audio.Models;

namespace NurFlac.Audio.Analyzers;

// AbstractProduct — base for all format-specific analyzers.
public abstract class BaseSpectralAnalyzer : ISpectralAnalyzer
{
    protected readonly IFfmpegTool FfmpegTool;
    private   readonly ScanConfig  _config;

    protected BaseSpectralAnalyzer(IFfmpegTool ffmpegTool)
    {
        FfmpegTool = ffmpegTool;
        _config    = DefaultConfig();
    }

    public async Task<SpectralAnalysisResult> AnalyzeAsync(string filePath, CancellationToken ct = default)
    {
        var samples = await FfmpegTool.ExtractPcmSamplesAsync(filePath, ct);
        return await SpectralAnalysisEngine.Instance.AnalyzeAsync(samples, SampleRate, _config);
    }

    // Template hook — subclasses declare their native sample rate.
    protected virtual int SampleRate => 44_100;

    // Template hook — subclasses may customize the scan config.
    protected virtual ScanConfig DefaultConfig() => new()
    {
        LosslessCutoffHz = 19_000,
        ThresholdDb      = -60.0,
        FftSize          = 4096
    };
}
