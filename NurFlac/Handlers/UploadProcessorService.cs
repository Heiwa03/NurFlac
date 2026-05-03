using NurFlac.Handlers.Models;

namespace NurFlac.Handlers;

public class UploadProcessorService : BackgroundService
{
    private readonly IUploadSessionQueue _queue;
    private readonly SingleAudioUploadCommand _uploadCommand;
    private readonly IUploadSessionCaretaker _caretaker;
    private readonly ILogger<UploadProcessorService> _logger;
    private readonly SemaphoreSlim _concurrencySemaphore = new(3, 3); // Max 3 parallel uploads

    public UploadProcessorService(
        IUploadSessionQueue queue,
        SingleAudioUploadCommand uploadCommand,
        IUploadSessionCaretaker caretaker,
        ILogger<UploadProcessorService> logger)
    {
        _queue = queue;
        _uploadCommand = uploadCommand;
        _caretaker = caretaker;
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
                var session = iterator.Current;

                // ATOMIC-LIKE TRANSITION (State Pattern):
                // Immediately transition the session to 'Processing' state in the Caretaker
                // to prevent other iterator refreshes from seeing it as 'Started'.
                session = session with { Status = UploadStatus.Processing };
                await _caretaker.SaveMementoAsync(session);

                foundWork = true;

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
                await _queue.WaitForWorkAsync(stoppingToken);
            }
            else
            {
                await Task.Delay(500, stoppingToken);
            }
        }
    }
}
