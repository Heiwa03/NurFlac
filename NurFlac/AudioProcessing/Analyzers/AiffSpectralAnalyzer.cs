namespace NurFlac.AudioProcessing.Analyzers;

public class AiffSpectralAnalyzer : BaseSpectralAnalyzer
{
    protected override Task<float[]> DecodeToPcmAsync(string filePath)
    {
        // TODO: Decode AIFF → PCM
        return Task.FromResult(new float[] { 1.0f });
    }
}