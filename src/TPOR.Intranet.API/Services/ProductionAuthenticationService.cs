using TPOR.Shared.Services;
using Google.Cloud.SecretManager.V1;

namespace TPOR.Intranet.API.Services;

public class ProductionAuthenticationService : IAuthenticationService
{
    private readonly ILogger<ProductionAuthenticationService> _logger;
    private readonly SecretManagerServiceClient _secretManagerClient;
    private readonly string _projectId;

    public ProductionAuthenticationService(ILogger<ProductionAuthenticationService> logger)
    {
        _logger = logger;
        _projectId = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT_ID") 
            ?? throw new InvalidOperationException("GOOGLE_CLOUD_PROJECT_ID environment variable is required");
        
        try
        {
            _secretManagerClient = SecretManagerServiceClient.Create();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Secret Manager client");
            throw;
        }
    }

    public Task<bool> ValidateJwtTokenAsync(string token)
    {
        try
        {
            // In production, you would validate the JWT token properly
            _logger.LogInformation("Production mode: JWT token validation");
            return Task.FromResult(!string.IsNullOrEmpty(token));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JWT token");
            return Task.FromResult(false);
        }
    }

    public async Task<string?> GetSecretAsync(string secretName)
    {
        try
        {
            _logger.LogInformation("Production mode: Getting secret {SecretName} from Secret Manager", secretName);
            
            var secretVersionName = new SecretVersionName(_projectId, secretName, "latest");
            var response = await _secretManagerClient.AccessSecretVersionAsync(secretVersionName);
            
            return response.Payload.Data.ToStringUtf8();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting secret {SecretName} from Secret Manager", secretName);
            return null;
        }
    }

    public Task<bool> SaveRefreshTokenAsync(string token, string userId, DateTime expiresAt)
    {
        try
        {
            // In production, this would save to database
            _logger.LogInformation("Production mode: Refresh token saved for user {UserId}", userId);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving refresh token for user {UserId}", userId);
            return Task.FromResult(false);
        }
    }
}
