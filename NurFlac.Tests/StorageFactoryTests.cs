using NurFlac.Storage;
using Microsoft.Extensions.Logging;

namespace NurFlac.Tests;

public class StorageFactoryTests
{
    private static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(_ => { });

    [Fact]
    public void WebDavFactory_CreatesCorrectTypes()
    {
        IStorageServiceFactory factory = new WebDavStorageFactory("https://dav.example.com", "user", "pass", LoggerFactory);

        Assert.IsType<WebDavStorageService>(factory.CreateFileUploader());
        Assert.IsType<WebDavStorageService>(factory.CreateDirectoryManager());
        Assert.IsType<WebDavStorageService>(factory.CreateDiagnostics());
        Assert.IsType<WebDavStorageService>(factory.CreateStorageService());
    }

    [Fact]
    public void SftpFactory_CreatesCorrectTypes()
    {
        IStorageServiceFactory factory = new SftpStorageFactory("sftp.example.com", "user");

        Assert.IsType<SftpStorageService>(factory.CreateFileUploader());
        Assert.IsType<SftpStorageService>(factory.CreateDirectoryManager());
        Assert.IsType<SftpStorageService>(factory.CreateDiagnostics());
        Assert.IsType<SftpStorageService>(factory.CreateStorageService());
    }

    [Fact]
    public void SambaFactory_CreatesCorrectTypes()
    {
        IStorageServiceFactory factory = new SambaStorageFactory(@"\\server\share");

        Assert.IsType<SambaStorageService>(factory.CreateFileUploader());
        Assert.IsType<SambaStorageService>(factory.CreateDirectoryManager());
        Assert.IsType<SambaStorageService>(factory.CreateDiagnostics());
        Assert.IsType<SambaStorageService>(factory.CreateStorageService());
    }

    [Fact]
    public async Task AllFactories_ProduceWorkingUploaders()
    {
        IStorageServiceFactory[] factories =
        [
            new SftpStorageFactory("sftp.example.com", "user"),
            new SambaStorageFactory(@"\\server\share"),
        ];

        var localTestFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(localTestFile, "nurflac storage test");

        foreach (var factory in factories)
        {
            var uploader = factory.CreateFileUploader();
            var result = await uploader.UploadFileAsync(localTestFile, "test.flac", "/music");
            Assert.True(result);
        }

        File.Delete(localTestFile);
    }

    [Fact]
    public async Task AllFactories_ProduceWorkingDiagnostics()
    {
        IStorageServiceFactory[] factories =
        [
            new SftpStorageFactory("sftp.example.com", "user"),
            new SambaStorageFactory(@"\\server\share"),
        ];

        foreach (var factory in factories)
        {
            var diag = factory.CreateDiagnostics();
            var result = await diag.CheckConnectionAsync();
            Assert.True(result);
        }
    }

    [Fact]
    public void FactoriesAreInterchangeable()
    {
        // Client code works against the abstract factory — provider is swappable
        IStorageServiceFactory factory = new WebDavStorageFactory("https://dav.example.com", "user", "pass", LoggerFactory);
        AssertFactoryProducesValidFamily(factory);

        factory = new SftpStorageFactory("sftp.example.com", "user");
        AssertFactoryProducesValidFamily(factory);
    }

    private static void AssertFactoryProducesValidFamily(IStorageServiceFactory factory)
    {
        Assert.NotNull(factory.CreateFileUploader());
        Assert.NotNull(factory.CreateDirectoryManager());
        Assert.NotNull(factory.CreateDiagnostics());
        Assert.NotNull(factory.CreateStorageService());
    }
}