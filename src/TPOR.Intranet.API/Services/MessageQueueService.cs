using TPOR.Shared.Services;
using TPOR.Shared.Models;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using System.Text.Json;

namespace TPOR.Intranet.API.Services;

public class MessageQueueService : IMessageQueueService
{
    private readonly ILogger<MessageQueueService> _logger;
    private readonly PublisherServiceApiClient? _publisher;
    private readonly string _topicName;
    private readonly string _projectId;
    private readonly bool _isProduction;

    public MessageQueueService(ILogger<MessageQueueService> logger)
    {
        _logger = logger;
        _projectId = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT_ID") ?? "tpor-project";
        _topicName = Environment.GetEnvironmentVariable("PUBSUB_TOPIC_NAME") ?? "file-processing-topic";
        _isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
        
        if (_isProduction)
        {
            try
            {
                _publisher = PublisherServiceApiClient.Create();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Pub/Sub publisher client");
                throw;
            }
        }
    }

    public async Task PublishMessageAsync(FileProcessingMessage message)
    {
        try
        {
            if (_isProduction && _publisher != null)
            {
                var topicName = TopicName.FromProjectTopic(_projectId, _topicName);
                
                var messageJson = JsonSerializer.Serialize(message);
                var pubsubMessage = new PubsubMessage
                {
                    Data = ByteString.CopyFromUtf8(messageJson)
                };

                await _publisher.PublishAsync(topicName, new[] { pubsubMessage });
                
                _logger.LogInformation("Message published successfully for file: {FileName}", message.FileName);
            }
            else
            {
                // 開發環境：模擬消息發布
                _logger.LogInformation("Development mode: Message would be published for file: {FileName}", message.FileName);
                await Task.Delay(100); // 模擬異步操作
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message for file: {FileName}", message.FileName);
            throw;
        }
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
