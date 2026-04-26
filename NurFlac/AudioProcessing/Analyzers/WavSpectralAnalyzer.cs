namespace NurFlac.AudioProcessing.Analyzers;

public class WavSpectralAnalyzer : BaseSpectralAnalyzer
{
    protected override Task<float[]> DecodeToPcmAsync(string filePath)
    {
        // TODO: Decode WAV → PCM
        return Task.FromResult(new float[] { 1.0f });
    }
}