namespace NurFlac.AudioProcessing.SpectralAnalysis.Models;

public class ScanConfig : IScanConfigPrototype
{
    public int FftSize { get; set; } = 2048;
    public double ThresholdDb { get; set; } = -60.0;
    public double LosslessCutoffHz { get; set; } = 20000;
    public int SampleDurationSeconds { get; set; } = 30;

    public IScanConfigPrototype Clone()
    {
        return new ScanConfig
        {
            FftSize = this.FftSize,
            ThresholdDb = this.ThresholdDb,
            LosslessCutoffHz = this.LosslessCutoffHz,
            SampleDurationSeconds = this.SampleDurationSeconds
        };
    }
}
