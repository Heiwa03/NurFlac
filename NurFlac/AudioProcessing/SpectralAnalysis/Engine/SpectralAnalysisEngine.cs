using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using NurFlac.AudioProcessing.SpectralAnalysis.Models;
using System.Numerics;

namespace NurFlac.AudioProcessing.SpectralAnalysis.Engine;

public sealed class SpectralAnalysisEngine : ISpectralAnalysisEngine
{
    private static readonly Lazy<SpectralAnalysisEngine> _instance = 
        new(() => new SpectralAnalysisEngine());

    public static SpectralAnalysisEngine Instance => _instance.Value;

    private SpectralAnalysisEngine() { }

    public Task<SpectralAnalysisResult> AnalyzeAsync(float[] samples, int sampleRate, ScanConfig config)
    {
        if (samples.Length < config.FftSize) 
            return Task.FromResult(new SpectralAnalysisResult(false, 0, config.LosslessCutoffHz, config.ThresholdDb, 0, "Insufficient samples"));

        int n = config.FftSize;
        var complexSamples = new Complex[n];
        int startOffset = Math.Max(0, (samples.Length / 2) - (n / 2));
        
        for (int i = 0; i < n; i++)
        {
            complexSamples[i] = new Complex(samples[startOffset + i], 0);
        }

        Fourier.Forward(complexSamples, FourierOptions.NoScaling);

        double hzPerBin = (double)sampleRate / n;
        
        double audibleEnergy = 0; // 1kHz - 10kHz
        double highFreqEnergy = 0; // 19kHz+
        double maxDetectedFreq = 0;

        int audibleStart = (int)(1000 / hzPerBin);
        int audibleEnd = (int)(10000 / hzPerBin);
        int highStart = (int)(19000 / hzPerBin);
        int highEnd = n / 2;

        for (int i = 0; i < n / 2; i++)
        {
            double magnitude = complexSamples[i].Magnitude;
            double db = 20 * Math.Log10(magnitude + 1e-9);

            if (db > config.ThresholdDb)
            {
                maxDetectedFreq = i * hzPerBin;
            }

            if (i >= audibleStart && i <= audibleEnd) audibleEnergy += magnitude;
            if (i >= highStart && i <= highEnd) highFreqEnergy += magnitude;
        }

        // Calculate energy ratio. 
        // True lossless has significant energy in the high frequency range relative to the audible range.
        // MP3s, even at 320kbps, have a massive drop-off (energy density) after their cutoff.
        double energyRatio = audibleEnergy > 0 ? (highFreqEnergy / (highEnd - highStart)) / (audibleEnergy / (audibleEnd - audibleStart)) : 0;
        
        // A very low ratio (e.g. < 0.01) usually indicates a transcode even if noise spikes cross the dB threshold.
        const double MinimumEnergyRatio = 0.005; 

        bool isTrueLossless = maxDetectedFreq >= config.LosslessCutoffHz && energyRatio > MinimumEnergyRatio;
        
        string note = isTrueLossless ? "High frequency energy density is consistent with lossless." 
                                   : energyRatio <= MinimumEnergyRatio ? "High frequency energy density too low (potential transcode)." 
                                   : "Frequency cutoff detected.";

        return Task.FromResult(new SpectralAnalysisResult(isTrueLossless, maxDetectedFreq, config.LosslessCutoffHz, config.ThresholdDb, energyRatio, note));
    }
}
