namespace NurFlac.AudioProcessing.Analyzers;

public class AlacSpectralAnalyzer : BaseSpectralAnalyzer
{
    protected override Task<float[]> DecodeToPcmAsync(string filePath) 
        => ExtractPcmViaFfmpegAsync(filePath);
}
