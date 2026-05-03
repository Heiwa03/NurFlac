using Xunit;
using NurFlac.AudioProcessing.SpectralAnalysis.Engine;
using NurFlac.AudioProcessing.SpectralAnalysis.Models;

namespace NurFlac.Tests;

public class SpectralAnalysisEngineTests
{
    [Fact]
    public void ScanConfig_Prototype_ClonesCorrectly()
    {
        var original = new ScanConfig { FftSize = 1024, ThresholdDb = -45.0 };
        var clone = (ScanConfig)original.Clone();

        Assert.NotSame(original, clone);
        Assert.Equal(original.FftSize, clone.FftSize);
        Assert.Equal(original.ThresholdDb, clone.ThresholdDb);
    }

    [Fact]
    public void ScanConfigBuilder_CreatesCorrectConfig()
    {
        var config = new ScanConfigBuilder()
            .WithHighResolution()
            .WithThreshold(-70.0)
            .Build();

        Assert.Equal(4096, config.FftSize);
        Assert.Equal(-70.0, config.ThresholdDb);
        Assert.Equal(60, config.SampleDurationSeconds);
    }

    [Fact]
    public void SpectralAnalysisEngine_IsSingleton()
    {
        var instance1 = SpectralAnalysisEngine.Instance;
        var instance2 = SpectralAnalysisEngine.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public async Task Engine_DetectsLossy_WhenSilence()
    {
        var samples = new float[2048]; // All zeros
        var config = new ScanConfig();
        
        var result = await SpectralAnalysisEngine.Instance.AnalyzeAsync(samples, 44100, config);
        
        Assert.False(result.IsTrueLossless);
    }

    [Fact]
    public async Task Engine_DetectsLossless_WhenFullSpectrum()
    {
        var rand = new Random();
        var samples = new float[2048];
        for (int i = 0; i < samples.Length; i++) 
            samples[i] = (float)(rand.NextDouble() * 2 - 1);

        var config = new ScanConfig { ThresholdDb = -100.0, LosslessCutoffHz = 15000 };
        
        var result = await SpectralAnalysisEngine.Instance.AnalyzeAsync(samples, 44100, config);
        
        Assert.True(result.IsTrueLossless);
    }
}
