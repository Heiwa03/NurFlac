using NurFlac.AudioProcessing;
using NurFlac.AudioProcessing.Interfaces;
using NurFlac.Validation;

namespace NurFlac.Tests;

public class ValidationPipelineTests
{
    private static readonly AudioFormatRegistry Registry = new();

    // ── PassthroughValidator ────────────────────────────────────

    [Fact]
    public async Task PassthroughValidator_AlwaysReturnsValid()
    {
        var validator = new PassthroughValidator();
        var context = MakeContext("song.flac", ".flac");

        var result = await validator.ValidateAsync(context);

        Assert.True(result.IsValid);
        Assert.Null(result.RejectionReason);
    }

    // ── ExtensionValidatorDecorator ─────────────────────────────

    [Theory]
    [InlineData(".flac")]
    [InlineData(".wav")]
    [InlineData(".alac")]
    [InlineData(".m4a")]
    [InlineData(".aiff")]
    [InlineData(".aif")]
    public async Task ExtensionValidator_Accepts_LosslessExtensions(string extension)
    {
        var validator = new ExtensionValidatorDecorator(new PassthroughValidator(), Registry);
        var context = MakeContext($"song{extension}", extension);

        var result = await validator.ValidateAsync(context);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(".mp3")]
    [InlineData(".aac")]
    [InlineData(".ogg")]
    [InlineData(".opus")]
    public async Task ExtensionValidator_Rejects_LossyExtensions(string extension)
    {
        var validator = new ExtensionValidatorDecorator(new PassthroughValidator(), Registry);
        var context = MakeContext($"song{extension}", extension);

        var result = await validator.ValidateAsync(context);

        Assert.False(result.IsValid);
        Assert.NotNull(result.RejectionReason);
    }

    [Theory]
    [InlineData(".wma")]
    [InlineData(".xyz")]
    [InlineData("")]
    public async Task ExtensionValidator_Rejects_UnknownExtensions(string extension)
    {
        var validator = new ExtensionValidatorDecorator(new PassthroughValidator(), Registry);
        var context = MakeContext("song.xyz", extension);

        var result = await validator.ValidateAsync(context);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ExtensionValidator_ShortCircuits_OnRejection_InnerNotCalled()
    {
        var inner = new TrackingValidator();
        var validator = new ExtensionValidatorDecorator(inner, Registry);
        var context = MakeContext("song.mp3", ".mp3");

        var result = await validator.ValidateAsync(context);

        Assert.False(result.IsValid);
        Assert.Equal(0, inner.CallCount);
    }

    [Fact]
    public async Task ExtensionValidator_CallsInner_WhenExtensionPasses()
    {
        var inner = new TrackingValidator();
        var validator = new ExtensionValidatorDecorator(inner, Registry);
        var context = MakeContext("song.flac", ".flac");

        await validator.ValidateAsync(context);

        Assert.Equal(1, inner.CallCount);
    }

    // ── MimeValidatorDecorator ──────────────────────────────────

    [Fact]
    public async Task MimeValidator_PassesThrough_WhenMimeIsNull()
    {
        var inner = new TrackingValidator();
        var validator = new MimeValidatorDecorator(inner, Registry);
        var context = MakeContext("song.flac", ".flac", mimeType: null);

        var result = await validator.ValidateAsync(context);

        Assert.True(result.IsValid);
        Assert.Equal(1, inner.CallCount);
    }

    [Theory]
    [InlineData("audio/flac")]
    [InlineData("audio/wav")]
    [InlineData("audio/aiff")]
    public async Task MimeValidator_Accepts_LosslessMimeType(string mimeType)
    {
        var validator = new MimeValidatorDecorator(new PassthroughValidator(), Registry);
        var context = MakeContext("song.flac", ".flac", mimeType);

        var result = await validator.ValidateAsync(context);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("audio/mpeg")]
    [InlineData("audio/aac")]
    [InlineData("audio/ogg")]
    public async Task MimeValidator_Rejects_LossyMimeType(string mimeType)
    {
        var validator = new MimeValidatorDecorator(new PassthroughValidator(), Registry);
        var context = MakeContext("song.mp3", ".mp3", mimeType);

        var result = await validator.ValidateAsync(context);

        Assert.False(result.IsValid);
        Assert.NotNull(result.RejectionReason);
    }

    [Fact]
    public async Task MimeValidator_Rejects_UnknownMimeType()
    {
        var validator = new MimeValidatorDecorator(new PassthroughValidator(), Registry);
        var context = MakeContext("song.xyz", ".xyz", "application/octet-stream");

        var result = await validator.ValidateAsync(context);

        Assert.False(result.IsValid);
    }

    // ── SpectralValidatorDecorator ──────────────────────────────

    [Fact]
    public async Task SpectralValidator_PassesThrough_WhenLocalPathIsNull()
    {
        var inner = new TrackingValidator();
        var processor = new StubAudioProcessor(returnsLossless: true);
        var validator = new SpectralValidatorDecorator(inner, processor);
        var context = MakeContext("song.flac", ".flac");

        var result = await validator.ValidateAsync(context);

        Assert.True(result.IsValid);
        Assert.Equal(1, inner.CallCount);
        Assert.Equal(0, processor.CallCount);
    }

    [Fact]
    public async Task SpectralValidator_Accepts_WhenProcessorReturnsTrue()
    {
        var processor = new StubAudioProcessor(returnsLossless: true);
        var validator = new SpectralValidatorDecorator(new PassthroughValidator(), processor);
        var context = MakeContext("song.flac", ".flac");
        context.LocalFilePath = "/tmp/song.flac";

        var result = await validator.ValidateAsync(context);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task SpectralValidator_Rejects_WhenProcessorReturnsFalse()
    {
        var processor = new StubAudioProcessor(returnsLossless: false);
        var validator = new SpectralValidatorDecorator(new PassthroughValidator(), processor);
        var context = MakeContext("song.flac", ".flac");
        context.LocalFilePath = "/tmp/song.flac";

        var result = await validator.ValidateAsync(context);

        Assert.False(result.IsValid);
        Assert.NotNull(result.RejectionReason);
    }

    // ── Full decorator chain ────────────────────────────────────

    [Fact]
    public async Task FullChain_RejectsOnStep1_InnerDecoratorsNotCalled()
    {
        var mimeInner = new TrackingValidator();
        var mimeValidator = new MimeValidatorDecorator(mimeInner, Registry);
        var extValidator = new ExtensionValidatorDecorator(mimeValidator, Registry);

        var context = MakeContext("song.mp3", ".mp3", "audio/mpeg");

        var result = await extValidator.ValidateAsync(context);

        Assert.False(result.IsValid);
        Assert.Equal(0, mimeInner.CallCount);
    }

    [Fact]
    public async Task FullChain_AcceptsLosslessFile()
    {
        var processor = new StubAudioProcessor(returnsLossless: true);
        ILosslessAudioValidator chain =
            new SpectralValidatorDecorator(
                new MimeValidatorDecorator(
                    new ExtensionValidatorDecorator(
                        new PassthroughValidator(), Registry),
                    Registry),
                processor);

        var context = MakeContext("song.flac", ".flac", "audio/flac");
        context.LocalFilePath = "/tmp/song.flac";

        var result = await chain.ValidateAsync(context);

        Assert.True(result.IsValid);
    }

    // ── Helpers ─────────────────────────────────────────────────

    private static AudioFileContext MakeContext(string fileName, string extension, string? mimeType = null)
        => new(fileName, extension, mimeType, telegramFileId: "test-id");

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

        public Task<bool> VerifyLosslessQualityAsync(string filePath)
        {
            CallCount++;
            return Task.FromResult(_returnsLossless);
        }

        public Task<string> ConvertToFlacAsync(string inputPath) => Task.FromResult(inputPath);
    }
}
