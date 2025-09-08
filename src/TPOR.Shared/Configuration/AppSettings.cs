namespace TPOR.Shared.Configuration;

public class AppSettings
{
    public DatabaseSettings Database { get; set; } = new();
    public StorageSettings Storage { get; set; } = new();
    public MessageQueueSettings MessageQueue { get; set; } = new();
    public AuthenticationSettings Authentication { get; set; } = new();
    public GoogleCloudSettings GoogleCloud { get; set; } = new();
}

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}

public class StorageSettings
{
    public string LocalPath { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
}

public class MessageQueueSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string SubscriptionName { get; set; } = string.Empty;
}

public class AuthenticationSettings
{
    public string JwtSecret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}

public class GoogleCloudSettings
{
    public string ProjectId { get; set; } = string.Empty;
    public string ServiceAccountKeyPath { get; set; } = string.Empty;
    public string SecretManagerSecretName { get; set; } = string.Empty;
}
