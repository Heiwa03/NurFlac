namespace NurFlac.AudioProcessing.SpectralAnalysis.Models;

public class ScanConfigBuilder
{
    private readonly ScanConfig _config = new();

    public ScanConfigBuilder WithHighResolution()
    {
        _config.FftSize = 4096;
        _config.SampleDurationSeconds = 60;
        return this;
    }

    public ScanConfigBuilder WithThreshold(double db)
    {
        _config.ThresholdDb = db;
        return this;
    }

    public ScanConfigBuilder WithCutoff(double hz)
    {
        _config.LosslessCutoffHz = hz;
        return this;
    }

    public ScanConfig Build() => (ScanConfig)_config.Clone();
}
