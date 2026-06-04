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
    // → runs: ffmpeg -ss 30 -t 30 -i <file> -f f32le -ac 1 -ar 44100 <temp>
    public async Task<float[]> ExtractPcmSamplesAsync(string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath)) return [];

        var tempPcm = Path.Combine(Path.GetTempPath(), $"nurflac_{Guid.NewGuid():N}.raw");

        var psi = new ProcessStartInfo
        {
            FileName               = "ffmpeg",
            Arguments              = $"-ss 30 -t 30 -i \"{filePath}\" -f f32le -ac 1 -ar 44100 \"{tempPcm}\" -y",
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true
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
}
