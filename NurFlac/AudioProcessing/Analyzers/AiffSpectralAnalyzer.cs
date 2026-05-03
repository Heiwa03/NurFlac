namespace NurFlac.AudioProcessing.Analyzers;

public class AiffSpectralAnalyzer : BaseSpectralAnalyzer
{
    protected override Task<float[]> DecodeToPcmAsync(string filePath) 
        => ExtractPcmViaFfmpegAsync(filePath);
}
