namespace NurFlac.AudioProcessing.Interfaces;

public interface IAudioProcessor
{
    Task<string> ConvertToFlacAsync(string inputPath);
    Task<bool> VerifyLosslessQualityAsync(string filePath);
}
