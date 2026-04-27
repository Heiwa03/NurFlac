using Microsoft.Extensions.Logging.Abstractions;
using NurFlac.Storage;

namespace NurFlac.Tests;

public class StorageServiceProxyTests
{
    private static StorageServiceProxy MakeProxy(RecordingStorageService inner)
        => new(inner, NullLogger<StorageServiceProxy>.Instance);

    // ── UploadFileAsync ─────────────────────────────────────────

    [Fact]
    public async Task UploadFileAsync_DelegatesToInner()
    {
        var inner = new RecordingStorageService();
        var proxy = MakeProxy(inner);

        await proxy.UploadFileAsync("/tmp/song.flac", "song.flac", "FLAC");

        Assert.Equal(1, inner.UploadCalls);
    }

    [Fact]
    public async Task UploadFileAsync_ReturnsTrue_WhenInnerReturnsTrue()
    {
        var proxy = MakeProxy(new RecordingStorageService(result: true));

        var result = await proxy.UploadFileAsync("/tmp/song.flac", "song.flac", string.Empty);

        Assert.True(result);
    }

    [Fact]
    public async Task UploadFileAsync_ReturnsFalse_WhenInnerReturnsFalse()
    {
        var proxy = MakeProxy(new RecordingStorageService(result: false));

        var result = await proxy.UploadFileAsync("/tmp/song.flac", "song.flac", string.Empty);

        Assert.False(result);
    }

    // ── CheckConnectionAsync ────────────────────────────────────

    [Fact]
    public async Task CheckConnectionAsync_DelegatesToInner()
    {
        var inner = new RecordingStorageService();
        var proxy = MakeProxy(inner);

        await proxy.CheckConnectionAsync();

        Assert.Equal(1, inner.CheckConnectionCalls);
    }

    [Fact]
    public async Task CheckConnectionAsync_ReturnsTrue_WhenInnerReturnsTrue()
    {
        var proxy = MakeProxy(new RecordingStorageService(result: true));

        var result = await proxy.CheckConnectionAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task CheckConnectionAsync_ReturnsFalse_WhenInnerReturnsFalse()
    {
        var proxy = MakeProxy(new RecordingStorageService(result: false));

        var result = await proxy.CheckConnectionAsync();

        Assert.False(result);
    }

    // ── CreateDirectoryAsync ────────────────────────────────────

    [Fact]
    public async Task CreateDirectoryAsync_DelegatesToInner()
    {
        var inner = new RecordingStorageService();
        var proxy = MakeProxy(inner);

        await proxy.CreateDirectoryAsync("FLAC");

        Assert.Equal(1, inner.CreateDirectoryCalls);
    }

    [Fact]
    public async Task CreateDirectoryAsync_ReturnsInnerResult()
    {
        var proxy = MakeProxy(new RecordingStorageService(result: false));

        var result = await proxy.CreateDirectoryAsync("FLAC");

        Assert.False(result);
    }

    // ── Proxy transparency ──────────────────────────────────────

    [Fact]
    public async Task Proxy_IsTransparent_AllThreeOperationsDelegate()
    {
        var inner = new RecordingStorageService();
        var proxy = MakeProxy(inner);

        await proxy.CheckConnectionAsync();
        await proxy.CreateDirectoryAsync("FLAC");
        await proxy.UploadFileAsync("/tmp/song.flac", "song.flac", "FLAC");

        Assert.Equal(1, inner.CheckConnectionCalls);
        Assert.Equal(1, inner.CreateDirectoryCalls);
        Assert.Equal(1, inner.UploadCalls);
    }

    // ── Test double ─────────────────────────────────────────────

    internal sealed class RecordingStorageService : IStorageService
    {
        private readonly bool _result;

        public int UploadCalls { get; private set; }
        public int CheckConnectionCalls { get; private set; }
        public int CreateDirectoryCalls { get; private set; }

        public RecordingStorageService(bool result = true) => _result = result;

        public Task<bool> UploadFileAsync(string filePath, string remoteFileName, string folderPath)
        {
            UploadCalls++;
            return Task.FromResult(_result);
        }

        public Task<bool> CheckConnectionAsync()
        {
            CheckConnectionCalls++;
            return Task.FromResult(_result);
        }

        public Task<bool> CreateDirectoryAsync(string folderPath)
        {
            CreateDirectoryCalls++;
            return Task.FromResult(_result);
        }
    }
}
