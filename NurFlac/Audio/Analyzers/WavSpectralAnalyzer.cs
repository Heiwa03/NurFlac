using NurFlac.Audio.Abstractions;

namespace NurFlac.Audio.Analyzers;

public sealed class WavSpectralAnalyzer(IFfmpegTool ffmpegTool) : BaseSpectralAnalyzer(ffmpegTool);
