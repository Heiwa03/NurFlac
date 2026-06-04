// ============================================================
// PATTERN: Adapter (Structural)
// Role   : Target — the interface our application expects.
//          FfmpegAdapter maps the external Process API onto this.
// ============================================================
namespace NurFlac.Audio.Abstractions;

public interface IFfmpegTool
{
    Task<float[]> ExtractPcmSamplesAsync(string filePath, CancellationToken ct = default);
}
