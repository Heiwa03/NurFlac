using NurFlac.AudioProcessing;
using NurFlac.Storage;
using NurFlac.Validation;

namespace NurFlac.Tests;

public class AudioLibraryStorageTests
{
    private static readonly AudioFormatRegistry Registry = new();

    // ── FlatAudioLibraryStorage ─────────────────────────────────

    [Fact]
    public async Task FlatStorage_UploadsToRootFolder()
    {
        var storage = new RecordingStorageService();
        var library = new FlatAudioLibraryStorage(storage);
        var context = MakeContext("song.flac", ".flac", localPath: "/tmp/song.flac");

        await library.UploadAudioAsync(context);

        Assert.Equal(string.Empty, storage.LastFolderPath);
    }

    [Fact]
    public async Task FlatStorage_UsesOriginalFileName()
    {
        var storage = new RecordingStorageService();
        var library = new FlatAudioLibraryStorage(storage);
        var context = MakeContext("my-track.flac", ".flac", localPath: "/tmp/my-track.flac");

        await library.UploadAudioAsync(context);

        Assert.Equal("my-track.flac", storage.LastRemoteFileName);
    }

    [Fact]
    public async Task FlatStorage_ReturnsInnerResult_OnSuccess()
    {
        var storage = new RecordingStorageService(uploadResult: true);
        var library = new FlatAudioLibraryStorage(storage);
        var context = MakeContext("song.flac", ".flac", localPath: "/tmp/song.flac");

        var result = await library.UploadAudioAsync(context);

        Assert.True(result);
    }

    [Fact]
    public async Task FlatStorage_ReturnsInnerResult_OnFailure()
    {
        var storage = new RecordingStorageService(uploadResult: false);
        var library = new FlatAudioLibraryStorage(storage);
        var context = MakeContext("song.flac", ".flac", localPath: "/tmp/song.flac");

        var result = await library.UploadAudioAsync(context);

        Assert.False(result);
    }

    // ── OrganizedAudioLibraryStorage ────────────────────────────

    [Theory]
    [InlineData(".flac", "FLAC")]
    [InlineData(".wav",  "WAV")]
    [InlineData(".alac", "ALAC")]
    [InlineData(".m4a",  "ALAC")]
    [InlineData(".aiff", "AIFF")]
    public async Task OrganizedStorage_UploadsToFormatSubfolder(string extension, string expectedFolder)
    {
        var storage = new RecordingStorageService();
        var library = new OrganizedAudioLibraryStorage(storage, Registry);
        var context = MakeContext($"song{extension}", extension, localPath: $"/tmp/song{extension}");

        await library.UploadAudioAsync(context);

        Assert.Equal(expectedFolder, storage.LastFolderPath);
    }

    [Fact]
    public async Task OrganizedStorage_UsesUnknownFolder_ForUnrecognizedExtension()
    {
        var storage = new RecordingStorageService();
        var library = new OrganizedAudioLibraryStorage(storage, Registry);
        var context = MakeContext("song.xyz", ".xyz", localPath: "/tmp/song.xyz");

        await library.UploadAudioAsync(context);

        Assert.Equal("UNKNOWN", storage.LastFolderPath);
    }

    [Fact]
    public async Task OrganizedStorage_UsesOriginalFileName()
    {
        var storage = new RecordingStorageService();
        var library = new OrganizedAudioLibraryStorage(storage, Registry);
        var context = MakeContext("my-track.wav", ".wav", localPath: "/tmp/my-track.wav");

        await library.UploadAudioAsync(context);

        Assert.Equal("my-track.wav", storage.LastRemoteFileName);
    }

    // ── Bridge: same implementor used by both abstractions ──────

    [Fact]
    public async Task BothAbstractions_DelegateToTheSameStorageImplementor()
    {
        var storage = new RecordingStorageService();
        var flat = new FlatAudioLibraryStorage(storage);
        var organized = new OrganizedAudioLibraryStorage(storage, Registry);
        var context = MakeContext("song.flac", ".flac", localPath: "/tmp/song.flac");

        await flat.UploadAudioAsync(context);
        await organized.UploadAudioAsync(context);

        Assert.Equal(2, storage.UploadCallCount);
    }

    // ── Helpers ─────────────────────────────────────────────────

    private static AudioFileContext MakeContext(string fileName, string extension, string? localPath = null)
    {
        var ctx = new AudioFileContext(fileName, extension, mimeType: null, telegramFileId: "test-id");
        ctx.LocalFilePath = localPath;
        return ctx;
    }

    private sealed class RecordingStorageService : IStorageService
    {
        private readonly bool _uploadResult;

        public string? LastFolderPath { get; private set; }
        public string? LastRemoteFileName { get; private set; }
        public int UploadCallCount { get; private set; }

        public RecordingStorageService(bool uploadResult = true) => _uploadResult = uploadResult;

        public Task<bool> UploadFileAsync(string filePath, string remoteFileName, string folderPath)
        {
            LastFolderPath = folderPath;
            LastRemoteFileName = remoteFileName;
            UploadCallCount++;
            return Task.FromResult(_uploadResult);
        }

        public Task<bool> CreateDirectoryAsync(string folderPath) => Task.FromResult(true);
        public Task<bool> CheckConnectionAsync() => Task.FromResult(true);
    }
}
