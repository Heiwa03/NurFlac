// ConcreteState: AlbumUpload — accumulates files, validates+records+uploads all on /album-done.
using NurFlac.Audio.Models;
using NurFlac.Album.Report;

namespace NurFlac.Album.States;

public sealed class AlbumUploadState : IAlbumState
{
    public string StateName => "AlbumUpload";

    public Task<string> HandleFileAsync(AlbumSession ctx, AudioFileContext file, CancellationToken ct)
    {
        ctx.PendingFiles.Add(file);
        return Task.FromResult(
            $"File \"{file.FileName}\" queued ({ctx.PendingFiles.Count} in batch). " +
            "Send /album-done when finished.");
    }

    public Task<string> HandleAlbumUploadCommandAsync(AlbumSession ctx, CancellationToken ct) =>
        Task.FromResult("Album session already active. Add files or send /album-done.");

    public async Task<string> HandleAlbumDoneCommandAsync(AlbumSession ctx, CancellationToken ct)
    {
        if (ctx.PendingFiles.Count == 0)
        {
            ctx.TransitionTo(new IdleState());
            return "No files were added to the album. Session cancelled.";
        }

        var builder = new AlbumReportBuilder(ctx.TelegramId);

        foreach (var file in ctx.PendingFiles)
        {
            var result = await ctx.Pipeline.ValidateAsync(file, ct);
            if (result.IsValid)
            {
                if (file.LocalFilePath is not null)
                {
                    await ctx.Ledger.RecordAsync(file.LocalFilePath, ctx.TelegramId, ct);
                    await ctx.Library.UploadAudioAsync(file, ct);
                }
                builder.AddSuccess(file.FileName);
            }
            else
            {
                builder.AddFailure(file.FileName, result.RejectionReason!);
            }
        }

        // Clean up all temp files for this batch.
        foreach (var file in ctx.PendingFiles)
        {
            if (file.LocalFilePath is not null && File.Exists(file.LocalFilePath))
                File.Delete(file.LocalFilePath);
        }

        ctx.TransitionTo(new IdleState());
        return builder.Build().ToMarkdown();
    }
}
