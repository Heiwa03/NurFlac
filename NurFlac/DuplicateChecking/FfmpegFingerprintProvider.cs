using System.Security.Cryptography;
using FFMpegCore;
using NurFlac.DuplicateChecking.Models;

namespace NurFlac.DuplicateChecking;

public sealed class FfmpegFingerprintProvider : IAudioFingerprintProvider
{
    public async Task<AudioFingerprint> CreateFingerprintAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var mediaInfo = await FFProbe.AnalyseAsync(filePath, cancellationToken: cancellationToken);

        await using var stream = File.OpenRead(filePath);
        var hashBytes = await SHA256.HashDataAsync(stream, cancellationToken);
        var hash = Convert.ToHexString(hashBytes);

        var codec = mediaInfo.PrimaryAudioStream?.CodecName ?? "unknown";
        var sampleRate = mediaInfo.PrimaryAudioStream?.SampleRateHz ?? 0;
        var durationMs = (long)mediaInfo.Duration.TotalMilliseconds;

        var fingerprintValue = $"ffmpeg:{codec}:{sampleRate}:{durationMs}:{hash}";
        return new AudioFingerprint(fingerprintValue, ProviderName: "ffmpeg");
    }
}
