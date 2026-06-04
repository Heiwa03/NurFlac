using NurFlac.Album;
using NurFlac.Audio.Facade;
using NurFlac.Audio.Models;
using NurFlac.Commands;
using NurFlac.Configuration;
using NurFlac.Ledger;
using NurFlac.Storage;
using NurFlac.Users;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NurFlac.Infrastructure.Telegram;

public sealed class UpdateRouter(
    CommandDispatcher     dispatcher,
    AlbumSessionManager   sessions,
    IUserService          userService,
    AudioPipelineFacade   pipeline,
    LedgerService         ledger,
    AudioLibraryStorage   audioLibrary,
    IBotConfiguration     config,
    ITelegramBotClient    botClient,
    ILogger<UpdateRouter> logger)
{
    private static readonly HttpClient _http = new();

    public async Task RouteAsync(Update update, CancellationToken ct)
    {
        if (update.Message is not { } msg) return;

        var userId   = msg.From?.Id ?? 0L;
        var username = msg.From?.Username ?? msg.From?.FirstName ?? "unknown";

        logger.LogDebug("[UPDATE] Type={Type} From={UserId}(@{Username}) ChatId={ChatId}",
            update.Type, userId, username, msg.Chat.Id);

        if (msg.Audio is not null || msg.Document is not null)
        {
            logger.LogDebug("[UPLOAD] File message detected from {UserId}", userId);
            await HandleFileMessageAsync(msg, userId, ct);
            return;
        }

        if (msg.Type == MessageType.Text && msg.Text?.StartsWith('/') == true)
        {
            logger.LogInformation("[CMD] {UserId}(@{Username}) → {Text}", userId, username, msg.Text);
            await dispatcher.DispatchAsync(msg, ct);
            return;
        }

        logger.LogDebug("[UPDATE] Ignored non-command text from {UserId}", userId);
    }

    private async Task HandleFileMessageAsync(Message msg, long userId, CancellationToken ct)
    {
        // ── Resolve file metadata ─────────────────────────────────────────
        string? mime    = msg.Document?.MimeType ?? msg.Audio?.MimeType;
        string  rawName = msg.Document?.FileName
                       ?? msg.Audio?.FileName
                       ?? (mime is not null ? $"audio{MimeToExtension(mime)}" : "audio_file");
        string  ext     = Path.GetExtension(rawName).ToLowerInvariant();

        if (string.IsNullOrEmpty(ext) && mime is not null)
            ext = MimeToExtension(mime);

        string fileId = (msg.Document?.FileId ?? msg.Audio?.FileId)!;

        logger.LogInformation(
            "[UPLOAD] File received — name='{Name}' ext='{Ext}' mime='{Mime}' fileId='{FileId}' from={UserId}",
            rawName, ext, mime ?? "(none)", fileId, userId);

        var context = new AudioFileContext(rawName, ext, mime, fileId);

        // ── Moderation check ──────────────────────────────────────────────
        var user = await userService.GetOrCreateAsync(userId, ct);
        logger.LogDebug("[UPLOAD] User status — id={UserId} status={Status} strikes={Strikes}",
            userId, user.Status, user.StrikeCount);

        if (user.IsBanned())
        {
            logger.LogInformation("[UPLOAD] Rejected — user {UserId} is banned", userId);
            await botClient.SendMessage(msg.Chat.Id, "You are permanently banned from using this bot.",
                cancellationToken: ct);
            return;
        }

        if (user.IsTimedOut())
        {
            var remaining = user.TimeoutUntil!.Value - DateTime.UtcNow;
            logger.LogInformation("[UPLOAD] Rejected — user {UserId} timed out for {Min:F0} more minutes",
                userId, remaining.TotalMinutes);
            await botClient.SendMessage(msg.Chat.Id,
                $"You are timed out. Try again in {remaining.TotalMinutes:F0} minutes.",
                cancellationToken: ct);
            return;
        }

        // ── Download file ─────────────────────────────────────────────────
        // Download before routing so the local path is available for spectral
        // validation in both single-file and album-batch modes.
        var localExt = string.IsNullOrEmpty(ext) ? ".bin" : ext;
        var tempPath = Path.Combine(Path.GetTempPath(), $"nurflac_{Guid.NewGuid():N}{localExt}");

        try
        {
            logger.LogDebug("[DOWNLOAD] Fetching file info for fileId={FileId}", fileId);
            var tgFile = await botClient.GetFile(fileId, ct);
            logger.LogDebug("[DOWNLOAD] Downloading to temp: {TempPath}", tempPath);

            await using (var fs = File.OpenWrite(tempPath))
                await DownloadAsync(tgFile.FilePath!, fs, ct);

            logger.LogInformation("[DOWNLOAD] Complete — {Bytes} bytes at {TempPath}",
                new FileInfo(tempPath).Length, tempPath);

            context.LocalFilePath = tempPath;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[DOWNLOAD] Failed for fileId={FileId}", fileId);
            if (File.Exists(tempPath)) File.Delete(tempPath);
            await botClient.SendMessage(msg.Chat.Id,
                "⚠️ Failed to download the file. Please try again.", cancellationToken: ct);
            return;
        }

        // ── Route to album session ────────────────────────────────────────
        var reply = await sessions.HandleFileAsync(userId, context, ct);
        logger.LogDebug("[UPLOAD] Session state reply: {Reply}", reply);

        if (reply == "File received for single validation.")
        {
            await ProcessSingleFileAsync(msg, userId, context, ct);
            return;
        }

        // Album mode: temp file is now owned by PendingFiles and will be
        // cleaned up by AlbumUploadState.HandleAlbumDoneCommandAsync.
        await botClient.SendMessage(msg.Chat.Id, reply, cancellationToken: ct);
    }

    private async Task ProcessSingleFileAsync(
        Message msg, long userId, AudioFileContext context, CancellationToken ct)
    {
        // File is already downloaded; context.LocalFilePath is set.
        await botClient.SendMessage(msg.Chat.Id, "⏳ Processing file, please wait…",
            cancellationToken: ct);

        try
        {
            // ── Validation pipeline ───────────────────────────────────────
            logger.LogInformation("[PIPELINE] Starting validation for '{File}' (ext={Ext}, mime={Mime})",
                context.FileName, context.Extension, context.MimeType ?? "(none)");

            var result = await pipeline.ValidateAsync(context, ct);

            if (!result.IsValid)
            {
                logger.LogInformation("[PIPELINE] REJECTED '{File}': {Reason}",
                    context.FileName, result.RejectionReason);
                await botClient.SendMessage(msg.Chat.Id,
                    $"❌ File rejected: {result.RejectionReason}", cancellationToken: ct);
                await userService.ApplyStrikeAsync(userId, 1, ct);
                var updated = await userService.GetOrCreateAsync(userId, ct);
                logger.LogInformation("[MODERATION] Strike applied — user={UserId} strikes={S} status={St}",
                    userId, updated.StrikeCount, updated.Status);
                return;
            }

            logger.LogInformation("[PIPELINE] PASSED '{File}'", context.FileName);

            // ── Duplicate check ───────────────────────────────────────────
            logger.LogDebug("[LEDGER] Checking for duplicate: '{File}'", context.FileName);
            if (await ledger.IsDuplicateAsync(context.LocalFilePath!, ct))
            {
                logger.LogInformation("[LEDGER] Duplicate detected for '{File}'", context.FileName);
                await botClient.SendMessage(msg.Chat.Id,
                    "⚠️ This file has already been uploaded.", cancellationToken: ct);
                return;
            }

            // ── Record in ledger ──────────────────────────────────────────
            await ledger.RecordAsync(context.LocalFilePath!, userId, ct);
            logger.LogInformation("[LEDGER] Recorded '{File}' for user {UserId}", context.FileName, userId);

            // ── Upload to storage ─────────────────────────────────────────
            logger.LogInformation("[STORAGE] Uploading '{File}' to library", context.FileName);
            var uploaded = await audioLibrary.UploadAudioAsync(context, ct);
            if (uploaded)
                logger.LogInformation("[STORAGE] '{File}' uploaded successfully", context.FileName);
            else
                logger.LogWarning("[STORAGE] Upload failed for '{File}' (recorded in ledger)", context.FileName);

            await botClient.SendMessage(msg.Chat.Id,
                uploaded
                    ? $"✅ \"{context.FileName}\" validated, recorded and uploaded to library."
                    : $"✅ \"{context.FileName}\" validated and recorded. (Storage upload failed — check logs)",
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[UPLOAD] Unhandled error processing '{File}'", context.FileName);
            await botClient.SendMessage(msg.Chat.Id,
                "⚠️ An internal error occurred while processing your file.", cancellationToken: ct);
        }
        finally
        {
            if (context.LocalFilePath is not null && File.Exists(context.LocalFilePath))
            {
                File.Delete(context.LocalFilePath);
                logger.LogDebug("[UPLOAD] Temp file cleaned up: {TempPath}", context.LocalFilePath);
            }
        }
    }

    private async Task DownloadAsync(string filePath, Stream dest, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(config.LocalApiPath) &&
            !string.IsNullOrEmpty(config.NginxUrl) &&
            filePath.StartsWith(config.LocalApiPath))
        {
            var url = filePath.Replace(config.LocalApiPath, config.NginxUrl);
            logger.LogDebug("[DOWNLOAD] Using local API nginx URL: {Url}", url);
            using var resp = await _http.GetStreamAsync(url, ct);
            await resp.CopyToAsync(dest, ct);
            return;
        }

        logger.LogDebug("[DOWNLOAD] Using Telegram Bot API download");
        await botClient.DownloadFile(filePath, dest, ct);
    }

    private static string MimeToExtension(string mime) => mime switch
    {
        "audio/flac" or "audio/x-flac"                => ".flac",
        "audio/wav"  or "audio/x-wav" or "audio/wave" => ".wav",
        "audio/aiff" or "audio/x-aiff"                => ".aiff",
        "audio/alac" or "audio/x-m4a" or "audio/mp4"  => ".m4a",
        "audio/mpeg" or "audio/mp3"                    => ".mp3",
        "audio/aac"                                    => ".aac",
        "audio/ogg"                                    => ".ogg",
        "audio/opus"                                   => ".opus",
        _                                              => string.Empty
    };
}
