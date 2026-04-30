using NurFlac.Handlers.Models;

namespace NurFlac.Handlers;

public class UploadProcessorService : BackgroundService
{
    private readonly IUploadSessionQueue _queue;
    private readonly SingleAudioUploadCommand _uploadCommand;
    private readonly ILogger<UploadProcessorService> _logger;
    private readonly SemaphoreSlim _concurrencySemaphore = new(3, 3); // Max 3 parallel uploads

    public UploadProcessorService(
        IUploadSessionQueue queue,
        SingleAudioUploadCommand uploadCommand,
        ILogger<UploadProcessorService> logger)
    {
        _queue = queue;
        _uploadCommand = uploadCommand;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Upload Processor Service is starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var iterator = _queue.GetFairIterator();

            bool foundWork = false;
            while (await iterator.MoveNextAsync())
            {
                foundWork = true;
                var session = iterator.Current;

                // Wait for a concurrency slot
                await _concurrencySemaphore.WaitAsync(stoppingToken);

                // Fire and forget the processing task
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _uploadCommand.ResumeSessionAsync(session);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing session {Id}", session.SessionId);
                    }
                    finally
                    {
                        _concurrencySemaphore.Release();
                    }
                }, stoppingToken);
            }

            if (!foundWork)
            {
                // Wait for a signal that new work has arrived
                await _queue.WaitForWorkAsync(stoppingToken);
            }
            else
            {
                // Brief pause to prevent tight loops if MoveNext returns immediately
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
