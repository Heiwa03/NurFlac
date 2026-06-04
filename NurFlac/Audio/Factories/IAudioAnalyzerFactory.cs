// ============================================================
// PATTERN: Abstract Factory (Creational)
// Role   : AbstractFactory — declares creation methods for the
//          families of audio analyzer products. Each concrete
//          factory corresponds to a codec category.
// ============================================================
using NurFlac.Audio.Abstractions;

namespace NurFlac.Audio.Factories;

public interface IAudioAnalyzerFactory
{
    string Category { get; }
    ISpectralAnalyzer CreateFlacAnalyzer();
    ISpectralAnalyzer CreateWavAnalyzer();
    ISpectralAnalyzer CreateAlacAnalyzer();
    ISpectralAnalyzer CreateAiffAnalyzer();
    ISpectralAnalyzer? CreateForExtension(string extension);
}
