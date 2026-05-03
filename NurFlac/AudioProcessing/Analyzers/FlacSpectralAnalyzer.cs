namespace NurFlac.AudioProcessing.Analyzers;

public class FlacSpectralAnalyzer : BaseSpectralAnalyzer
{
    protected override Task<float[]> DecodeToPcmAsync(string filePath) 
        => ExtractPcmViaFfmpegAsync(filePath);
}
