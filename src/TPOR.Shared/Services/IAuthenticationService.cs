namespace TPOR.Shared.Services;

public interface IAuthenticationService
{
    Task<bool> ValidateJwtTokenAsync(string token);
    Task<string?> GetSecretAsync(string secretName);
    Task<bool> SaveRefreshTokenAsync(string token, string userId, DateTime expiresAt);
}
