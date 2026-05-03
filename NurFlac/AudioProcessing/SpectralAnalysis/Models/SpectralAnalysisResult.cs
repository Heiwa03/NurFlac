namespace NurFlac.AudioProcessing.SpectralAnalysis.Models;

public record SpectralAnalysisResult(
    bool IsTrueLossless,
    double DetectedCutoffHz,
    double RequiredCutoffHz,
    double ThresholdDb,
    double HighFreqEnergyRatio,
    string AnalysisNote = ""
);
