using TPOR.Shared.Services;
using TPOR.Shared.Models;
using System.Text.Json;

namespace TPOR.Intranet.Worker.Services;

public class WorkerMessageQueueService : IMessageQueueService
{
    private readonly ILogger<WorkerMessageQueueService> _logger;
    private readonly string _subscriptionName;
    private readonly string _projectId;
    private readonly bool _isProduction;

    public WorkerMessageQueueService(ILogger<WorkerMessageQueueService> logger)
    {
        _logger = logger;
        _projectId = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT_ID") ?? "tpor-project";
        _subscriptionName = Environment.GetEnvironmentVariable("PUBSUB_SUBSCRIPTION_NAME") ?? "file-processing-subscription";
        _isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
        
        if (_isProduction)
        {
            try
            {
                // Only create Google Cloud client in production
                var subscriber = Google.Cloud.PubSub.V1.SubscriberServiceApiClient.Create();
                _logger.LogInformation("Google Cloud Pub/Sub subscriber client created for production");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Pub/Sub subscriber client");
                throw;
            }
        }
        else
        {
            _logger.LogInformation("Development mode: Using mock message queue service");
        }
    }

    public Task PublishMessageAsync(FileProcessingMessage message)
    {
        // This method is typically used by the API service
        // For the worker service, we only receive messages
        throw new NotImplementedException("PublishMessageAsync is not implemented in the Worker service. Use the API service instead.");
    }

    public async Task<FileProcessingMessage?> ReceiveMessageAsync()
    {
        if (!_isProduction)
        {
            // Development mode: return null (no messages to process)
            _logger.LogDebug("Development mode: No messages to process");
            return null;
        }

        try
        {
            var subscriber = Google.Cloud.PubSub.V1.SubscriberServiceApiClient.Create();
            var subscriptionName = Google.Cloud.PubSub.V1.SubscriptionName.FromProjectSubscription(_projectId, _subscriptionName);
            
            var response = await subscriber.PullAsync(subscriptionName, maxMessages: 1);
            
            if (response.ReceivedMessages.Count == 0)
            {
                return null;
            }

            var receivedMessage = response.ReceivedMessages[0];
            var messageData = receivedMessage.Message.Data.ToStringUtf8();
            
            var message = JsonSerializer.Deserialize<FileProcessingMessage>(messageData);
            if (message != null)
            {
                // Store the message ID for acknowledgment
                message.ProcessedAt = DateTime.UtcNow;
            }

            // Acknowledge the message
            await subscriber.AcknowledgeAsync(subscriptionName, new[] { receivedMessage.AckId });
            
            _logger.LogInformation("Message received and acknowledged: {MessageId}", receivedMessage.Message.MessageId);
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving message from queue");
            return null;
        }
    }

    public async Task AcknowledgeMessageAsync(string messageId)
    {
        // Messages are automatically acknowledged in ReceiveMessageAsync
        // This method is kept for interface compatibility
        await Task.CompletedTask;
    }
}
