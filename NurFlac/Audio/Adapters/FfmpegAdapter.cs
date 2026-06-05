// ============================================================
// PATTERN: Adapter (Structural)
// Role   : Adaptee wrapper — adapts the Linux `ffmpeg` binary
//          (invoked via System.Diagnostics.Process) to the
//          IFfmpegTool (Target) interface expected by the app.
// ============================================================
using NurFlac.Audio.Abstractions;
using System.Diagnostics;

namespace NurFlac.Audio.Adapters;

public sealed class FfmpegAdapter : IFfmpegTool
{
    // Adapter translates the call: IFfmpegTool.ExtractPcmSamplesAsync()
    // → runs: ffmpeg [-ss <seek>] -t <take> -i <file> -f f32le -ac 1 -ar 44100 <temp>
    // Seek is only applied for files ≥ 60 s so short clips are never skipped past.
    public async Task<float[]> ExtractPcmSamplesAsync(string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath)) return [];

        var duration   = await ProbeDurationAsync(filePath, ct);
        var seek       = duration >= 60.0 ? 30.0 : 0.0;
        var take       = duration > 0 ? Math.Min(30.0, duration - seek) : 60.0;

        var seekArg    = seek > 0 ? $"-ss {seek:F1} " : string.Empty;
        var tempPcm    = Path.Combine(Path.GetTempPath(), $"nurflac_{Guid.NewGuid():N}.raw");

        var psi = new ProcessStartInfo
        {
            FileName              = "ffmpeg",
            Arguments             = $"{seekArg}-t {take:F1} -i \"{filePath}\" -f f32le -ac 1 -ar 44100 \"{tempPcm}\" -y",
            RedirectStandardError = true,
            UseShellExecute       = false,
            CreateNoWindow        = true
        };

        try
        {
            using var process = Process.Start(psi);
            if (process is null) return [];
            await process.WaitForExitAsync(ct);

            if (!File.Exists(tempPcm)) return [];

            var bytes  = await File.ReadAllBytesAsync(tempPcm, ct);
            var floats = new float[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
            return floats;
        }
        catch (OperationCanceledException) { throw; }
        catch { return []; }
        finally
        {
            if (File.Exists(tempPcm)) File.Delete(tempPcm);
        }
    }

    // ffprobe ships alongside ffmpeg — returns 0 on any failure so callers degrade gracefully.
    private static async Task<double> ProbeDurationAsync(string filePath, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName               = "ffprobe",
            Arguments              = $"-v quiet -show_entries format=duration -of csv=p=0 \"{filePath}\"",
            RedirectStandardOutput = true,
            UseShellExecute        = false,
            CreateNoWindow         = true
        };
        try
        {
            using var proc = Process.Start(psi);
            if (proc is null) return 0.0;
            var output = await proc.StandardOutput.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);
            return double.TryParse(output.Trim(),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var d) ? d : 0.0;
        }
        catch { return 0.0; }
    }
}
