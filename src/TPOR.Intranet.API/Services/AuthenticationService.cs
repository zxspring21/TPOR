using TPOR.Shared.Services;
using TPOR.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Google.Cloud.SecretManager.V1;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace TPOR.Intranet.API.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly TporDbContext _context;
    private readonly SecretManagerServiceClient? _secretManagerClient;
    private readonly string _projectId;
    private readonly bool _isProduction;

    public AuthenticationService(ILogger<AuthenticationService> logger, TporDbContext context)
    {
        _logger = logger;
        _context = context;
        _projectId = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT_ID") ?? "tpor-project";
        _isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
        
        if (_isProduction)
        {
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
    }

    public async Task<bool> ValidateJwtTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecret = await GetSecretAsync("jwt-secret");
            
            if (string.IsNullOrEmpty(jwtSecret))
            {
                _logger.LogError("JWT secret not found");
                return false;
            }

            var key = Encoding.UTF8.GetBytes(jwtSecret);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JWT token");
            return false;
        }
    }

    public async Task<string?> GetSecretAsync(string secretName)
    {
        try
        {
            if (_isProduction && _secretManagerClient != null)
            {
                var secretNameResource = SecretName.FromProjectSecret(_projectId, secretName);
                var secretVersionName = new SecretVersionName(secretNameResource.ProjectId, secretNameResource.SecretId, "latest");
                
                var response = await _secretManagerClient.AccessSecretVersionAsync(secretVersionName);
                return response.Payload.Data.ToStringUtf8();
            }
            else
            {
                // 開發環境：從環境變量或配置文件獲取
                return Environment.GetEnvironmentVariable("JWT_SECRET") 
                    ?? "dev-super-secret-jwt-key-that-is-at-least-32-characters-long";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secret: {SecretName}", secretName);
            return null;
        }
    }

    public async Task<bool> SaveRefreshTokenAsync(string token, string userId, DateTime expiresAt)
    {
        try
        {
            var refreshToken = new TPOR.Shared.Models.RefRefreshToken
            {
                Token = token,
                UserId = userId,
                ExpiresAt = expiresAt,
                Name = $"RefreshToken_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}",
                CreatedBy = userId,
                ModifiedBy = userId
            };

            _context.RefRefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Refresh token saved successfully for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving refresh token for user: {UserId}", userId);
            return false;
        }
    }
}
