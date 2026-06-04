using MathNet.Numerics.IntegralTransforms;
using NurFlac.Audio.Models;
using System.Numerics;

namespace NurFlac.Audio.Engine;

// Internal Singleton — single FFT engine shared by all analyzers.
public sealed class SpectralAnalysisEngine : ISpectralAnalysisEngine
{
    private static readonly Lazy<SpectralAnalysisEngine> _instance =
        new(static () => new SpectralAnalysisEngine(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static ISpectralAnalysisEngine Instance => _instance.Value;

    private SpectralAnalysisEngine() { }

    public Task<SpectralAnalysisResult> AnalyzeAsync(float[] samples, int sampleRate, ScanConfig config)
    {
        if (samples.Length < config.FftSize)
            return Task.FromResult(new SpectralAnalysisResult(
                false, 0, config.LosslessCutoffHz, config.ThresholdDb, 0,
                "Insufficient samples for FFT analysis."));

        int n = config.FftSize;
        var fft = new Complex[n];
        int offset = Math.Max(0, samples.Length / 2 - n / 2);

        for (int i = 0; i < n; i++)
            fft[i] = new Complex(samples[offset + i], 0);

        Fourier.Forward(fft, MathNet.Numerics.IntegralTransforms.FourierOptions.NoScaling);

        double hzPerBin    = (double)sampleRate / n;
        double audibleEnergy = 0, highFreqEnergy = 0, maxDetectedHz = 0;

        int audStart  = (int)(1_000  / hzPerBin);
        int audEnd    = (int)(10_000 / hzPerBin);
        int highStart = (int)(19_000 / hzPerBin);
        int highEnd   = n / 2;

        for (int i = 0; i < n / 2; i++)
        {
            double mag = fft[i].Magnitude;
            double db  = 20 * Math.Log10(mag + 1e-9);

            if (db > config.ThresholdDb) maxDetectedHz = i * hzPerBin;
            if (i >= audStart  && i <= audEnd)  audibleEnergy   += mag;
            if (i >= highStart && i <= highEnd) highFreqEnergy  += mag;
        }

        int audBins  = audEnd  - audStart  + 1;
        int highBins = highEnd - highStart + 1;
        double ratio = audibleEnergy > 0
            ? (highFreqEnergy / highBins) / (audibleEnergy / audBins)
            : 0;

        const double MinRatio = 0.005;
        bool lossless = maxDetectedHz >= config.LosslessCutoffHz && ratio > MinRatio;

        string note = lossless
            ? "High-frequency energy density is consistent with true lossless audio."
            : ratio <= MinRatio
                ? "High-frequency energy density too low — likely a transcode."
                : $"Frequency cutoff detected at {maxDetectedHz:F0} Hz.";

        return Task.FromResult(new SpectralAnalysisResult(
            lossless, maxDetectedHz, config.LosslessCutoffHz, config.ThresholdDb, ratio, note));
    }
}
