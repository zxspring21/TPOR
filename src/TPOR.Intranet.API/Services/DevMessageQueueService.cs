using TPOR.Shared.Services;
using TPOR.Shared.Models;

namespace TPOR.Intranet.API.Services;

public class DevMessageQueueService : IMessageQueueService
{
    private readonly ILogger<DevMessageQueueService> _logger;

    public DevMessageQueueService(ILogger<DevMessageQueueService> logger)
    {
        _logger = logger;
    }

    public async Task PublishMessageAsync(FileProcessingMessage message)
    {
        // Development mode: just log the message
        _logger.LogInformation("Development mode: Message would be published for file: {FileName}", message.FileName);
        await Task.Delay(100); // Simulate async operation
    }

    public Task<FileProcessingMessage?> ReceiveMessageAsync()
    {
        // This method is typically used by the worker service
        // For the API service, we only publish messages
        throw new NotImplementedException("ReceiveMessageAsync is not implemented in the API service. Use the Worker service instead.");
    }

    public Task AcknowledgeMessageAsync(string messageId)
    {
        // This method is typically used by the worker service
        // For the API service, we only publish messages
        throw new NotImplementedException("AcknowledgeMessageAsync is not implemented in the API service. Use the Worker service instead.");
    }
}
