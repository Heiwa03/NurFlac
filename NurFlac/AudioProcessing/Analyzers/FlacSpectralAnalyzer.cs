namespace NurFlac.AudioProcessing.Analyzers;

public class FlacSpectralAnalyzer : BaseSpectralAnalyzer
{
    protected override Task<float[]> DecodeToPcmAsync(string filePath)
    {
        // TODO: Decode FLAC → PCM via FFmpeg / NAudio
        return Task.FromResult(new float[] { 1.0f });
    }
}