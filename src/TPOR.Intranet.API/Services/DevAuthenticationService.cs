using TPOR.Shared.Services;

namespace TPOR.Intranet.API.Services;

public class DevAuthenticationService : IAuthenticationService
{
    private readonly ILogger<DevAuthenticationService> _logger;

    public DevAuthenticationService(ILogger<DevAuthenticationService> logger)
    {
        _logger = logger;
    }

    public Task<bool> ValidateJwtTokenAsync(string token)
    {
        // Development mode: simple validation
        _logger.LogInformation("Development mode: JWT token validation");
        return Task.FromResult(!string.IsNullOrEmpty(token));
    }

    public Task<string?> GetSecretAsync(string secretName)
    {
        // Development mode: return hardcoded secret
        _logger.LogInformation("Development mode: Getting secret {SecretName}", secretName);
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET") 
            ?? "dev-super-secret-jwt-key-that-is-at-least-32-characters-long";
        return Task.FromResult(secret);
    }

    public Task<bool> SaveRefreshTokenAsync(string token, string userId, DateTime expiresAt)
    {
        // Development mode: just log
        _logger.LogInformation("Development mode: Refresh token saved for user {UserId}", userId);
        return Task.FromResult(true);
    }
}
