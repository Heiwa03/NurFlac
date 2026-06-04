using NurFlac.Audio.Abstractions;

namespace NurFlac.Audio.Analyzers;

public sealed class AiffSpectralAnalyzer(IFfmpegTool ffmpegTool) : BaseSpectralAnalyzer(ffmpegTool);
