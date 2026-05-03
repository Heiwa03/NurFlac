using Microsoft.Extensions.Logging.Abstractions;
using NurFlac.AudioProcessing;
using NurFlac.AudioProcessing.Interfaces;
using NurFlac.AudioProcessing.SpectralAnalysis.Models;
using NurFlac.Validation;

namespace NurFlac.Tests;

public class ValidationPipelineTests
{
    private static readonly AudioFormatRegistry Registry = new();

    [Fact]
    public async Task ExtensionValidator_RejectsUnsupportedExtension()
    {
        var validator = new ExtensionValidatorDecorator(new PassthroughValidator(), Registry);
        var context = MakeContext("song.txt", ".txt");

        var result = await validator.ValidateAsync(context);

        Assert.False(result.IsValid);
        Assert.Contains("Unsupported", result.RejectionReason);
    }

    [Fact]
    public async Task ExtensionValidator_AcceptsSupportedExtension()
    {
        var validator = new ExtensionValidatorDecorator(new PassthroughValidator(), Registry);
        var context = MakeContext("song.flac", ".flac");

        var result = await validator.ValidateAsync(context);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task MimeValidator_RejectsUnsupportedMime()
    {
        var validator = new MimeValidatorDecorator(new PassthroughValidator(), Registry);
        var context = MakeContext("song.flac", ".flac", "text/plain");

        var result = await validator.ValidateAsync(context);

        Assert.False(result.IsValid);
        Assert.Contains("Unrecognized", result.RejectionReason);
    }

    [Fact]
    public async Task MimeValidator_AcceptsSupportedMime()
    {
        var validator = new MimeValidatorDecorator(new PassthroughValidator(), Registry);
        var context = MakeContext("song.flac", ".flac", "audio/flac");

        var result = await validator.ValidateAsync(context);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task SpectralValidator_SkipsIfFileNotDownloaded()
    {
        var stub = new StubAudioProcessor(true);
        var validator = new SpectralValidatorDecorator(new PassthroughValidator(), stub, NullLogger<SpectralValidatorDecorator>.Instance);
        var context = MakeContext("song.flac", ".flac");

        var result = await validator.ValidateAsync(context);

        Assert.True(result.IsValid);
        Assert.Equal(0, stub.CallCount);
    }

    [Fact]
    public async Task SpectralValidator_RejectsLossySpectral()
    {
        var stub = new StubAudioProcessor(false);
        var validator = new SpectralValidatorDecorator(new PassthroughValidator(), stub, NullLogger<SpectralValidatorDecorator>.Instance);
        var context = MakeContext("song.flac", ".flac");
        context.LocalFilePath = "dummy.flac";

        var result = await validator.ValidateAsync(context);

        Assert.False(result.IsValid);
        Assert.Contains("Spectral analysis indicates", result.RejectionReason);
        Assert.Equal(1, stub.CallCount);
    }

    [Fact]
    public async Task FullPipeline_WorksCorrectly()
    {
        var stub = new StubAudioProcessor(true);
        var pipeline = new SpectralValidatorDecorator(
            new MimeValidatorDecorator(
                new ExtensionValidatorDecorator(
                    new PassthroughValidator(),
                    Registry),
                Registry),
            stub,
            NullLogger<SpectralValidatorDecorator>.Instance);

        var context = MakeContext("song.flac", ".flac", "audio/flac");
        context.LocalFilePath = "dummy.flac";

        var result = await pipeline.ValidateAsync(context);

        Assert.True(result.IsValid);
        Assert.Equal(1, stub.CallCount);
    }

    [Fact]
    public async Task FullPipeline_StopsEarlyOnExtensionFailure()
    {
        var tracking = new TrackingValidator();
        var pipeline = new SpectralValidatorDecorator(
            new MimeValidatorDecorator(
                new ExtensionValidatorDecorator(
                    tracking,
                    Registry),
                Registry),
            new StubAudioProcessor(true),
            NullLogger<SpectralValidatorDecorator>.Instance);

        var context = MakeContext("song.txt", ".txt");

        var result = await pipeline.ValidateAsync(context);

        Assert.False(result.IsValid);
        Assert.Equal(0, tracking.CallCount);
    }

    private static AudioFileContext MakeContext(string fileName, string extension, string? mimeType = null)
    {
        return new AudioFileContext(fileName, extension, mimeType, "dummy-id");
    }

    private sealed class TrackingValidator : ILosslessAudioValidator
    {
        public int CallCount { get; private set; }

        public Task<ValidationResult> ValidateAsync(AudioFileContext context, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(ValidationResult.Valid());
        }
    }

    private sealed class StubAudioProcessor : IAudioProcessor
    {
        private readonly bool _returnsLossless;
        public int CallCount { get; private set; }

        public StubAudioProcessor(bool returnsLossless) => _returnsLossless = returnsLossless;

        public Task<SpectralAnalysisResult> AnalyzeLosslessQualityAsync(string filePath)
        {
            CallCount++;
            return Task.FromResult(new SpectralAnalysisResult(_returnsLossless, _returnsLossless ? 22050 : 15000, 19000, -60, 0.1));
        }

        public Task<bool> VerifyLosslessQualityAsync(string filePath)
        {
            return Task.FromResult(_returnsLossless);
        }

        public Task<string> ConvertToFlacAsync(string inputPath) => Task.FromResult(inputPath);
    }
}
