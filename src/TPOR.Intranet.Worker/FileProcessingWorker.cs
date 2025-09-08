using TPOR.Shared.Services;
using TPOR.Intranet.Worker.Services;

namespace TPOR.Intranet.Worker;

public class FileProcessingWorker : BackgroundService
{
    private readonly ILogger<FileProcessingWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public FileProcessingWorker(ILogger<FileProcessingWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("File Processing Worker started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var messageQueueService = scope.ServiceProvider.GetRequiredService<IMessageQueueService>();
                var fileProcessingService = scope.ServiceProvider.GetRequiredService<IFileProcessingService>();

                // Poll for messages
                var message = await messageQueueService.ReceiveMessageAsync();
                
                if (message != null)
                {
                    _logger.LogInformation("Processing file: {FileName}", message.FileName);
                    
                    var success = await fileProcessingService.ProcessFileAsync(message);
                    
                    if (success)
                    {
                        _logger.LogInformation("Successfully processed file: {FileName}", message.FileName);
                    }
                    else
                    {
                        _logger.LogError("Failed to process file: {FileName}", message.FileName);
                    }
                }
                else
                {
                    // No messages available, wait before polling again
                    await Task.Delay(5001, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing messages");
                await Task.Delay(10000, stoppingToken); // Wait longer on error
            }
        }

        _logger.LogInformation("File Processing Worker stopped at: {time}", DateTimeOffset.Now);
    }
}
