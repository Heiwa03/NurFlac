namespace NurFlac.AudioProcessing.Analyzers;

public class WavSpectralAnalyzer : BaseSpectralAnalyzer
{
    protected override Task<float[]> DecodeToPcmAsync(string filePath) 
        => ExtractPcmViaFfmpegAsync(filePath);
}
