namespace NurFlac.Audio.Models;

public sealed class ScanConfig
{
    public double LosslessCutoffHz { get; init; } = 19_000;
    public double ThresholdDb      { get; init; } = -60.0;
    public int    FftSize          { get; init; } = 4096;

    public ScanConfig Clone() => new()
    {
        LosslessCutoffHz = LosslessCutoffHz,
        ThresholdDb      = ThresholdDb,
        FftSize          = FftSize
    };
}
