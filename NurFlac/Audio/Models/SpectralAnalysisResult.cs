namespace NurFlac.Audio.Models;

public sealed record SpectralAnalysisResult(
    bool   IsTrueLossless,
    double DetectedCutoffHz,
    double RequiredCutoffHz,
    double ThresholdDb,
    double HighFreqEnergyRatio,
    string AnalysisNote);
