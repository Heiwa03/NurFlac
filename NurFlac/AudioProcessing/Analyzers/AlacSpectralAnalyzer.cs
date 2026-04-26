namespace NurFlac.AudioProcessing.Analyzers;

public class AlacSpectralAnalyzer : BaseSpectralAnalyzer
{
    protected override Task<float[]> DecodeToPcmAsync(string filePath)
    {
        // TODO: Decode ALAC → PCM via FFmpeg
        return Task.FromResult(new float[] { 1.0f });
    }
}