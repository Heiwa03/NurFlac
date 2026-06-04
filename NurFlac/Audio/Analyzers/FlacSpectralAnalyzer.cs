using NurFlac.Audio.Abstractions;

namespace NurFlac.Audio.Analyzers;

public sealed class FlacSpectralAnalyzer(IFfmpegTool ffmpegTool) : BaseSpectralAnalyzer(ffmpegTool);
