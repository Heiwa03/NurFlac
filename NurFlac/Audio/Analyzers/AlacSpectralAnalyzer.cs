using NurFlac.Audio.Abstractions;

namespace NurFlac.Audio.Analyzers;

public sealed class AlacSpectralAnalyzer(IFfmpegTool ffmpegTool) : BaseSpectralAnalyzer(ffmpegTool);
