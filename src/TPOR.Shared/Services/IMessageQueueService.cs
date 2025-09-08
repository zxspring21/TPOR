using TPOR.Shared.Models;

namespace TPOR.Shared.Services;

public interface IMessageQueueService
{
    Task PublishMessageAsync(FileProcessingMessage message);
    Task<FileProcessingMessage?> ReceiveMessageAsync();
    Task AcknowledgeMessageAsync(string messageId);
}
