using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TPOR.Shared.Services;
using TPOR.Shared.Configuration;
using Microsoft.Extensions.Options;

namespace TPOR.Intranet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthenticationSettings _authSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IOptions<AppSettings> appSettings,
        ILogger<AuthController> logger)
    {
        _authSettings = appSettings.Value.Authentication;
        _logger = logger;
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        try
        {
            var envJwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
            var configJwtSecret = _authSettings?.JwtSecret;
            var finalJwtSecret = envJwtSecret ?? configJwtSecret;
            
            return Ok(new
            {
                message = "Auth controller is working",
                jwtSecret = string.IsNullOrEmpty(finalJwtSecret) ? "NOT_CONFIGURED" : "CONFIGURED",
                jwtSecretValue = finalJwtSecret ?? "NULL",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                envJwtSecret = envJwtSecret ?? "NOT_SET",
                configJwtSecret = configJwtSecret ?? "NOT_SET"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test endpoint");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // In a real application, you would validate credentials against a database
            // For this example, we'll use a simple validation
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            // Get credentials based on environment
            var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
            string validUsername, validPassword;

            if (isProduction)
            {
                // In production, get credentials from Secret Manager
                var authService = HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
                var usernameSecretName = Environment.GetEnvironmentVariable("AUTH_USERNAME_SECRET_NAME") ?? "tpor-auth-username";
                var passwordSecretName = Environment.GetEnvironmentVariable("AUTH_PASSWORD_SECRET_NAME") ?? "tpor-auth-password";
                
                validUsername = await authService.GetSecretAsync(usernameSecretName) ?? "admin";
                validPassword = await authService.GetSecretAsync(passwordSecretName) ?? "password";
            }
            else
            {
                // In development, get credentials from environment variables
                validUsername = Environment.GetEnvironmentVariable("AUTH_USERNAME") ?? "admin";
                validPassword = Environment.GetEnvironmentVariable("AUTH_PASSWORD") ?? "password";
            }

            // Simple validation - in production, use proper authentication
            if (request.Username != validUsername || request.Password != validPassword)
            {
                return Unauthorized("Invalid credentials.");
            }

            // Check JWT secret configuration based on environment
            string jwtSecret;
            if (isProduction)
            {
                // In production, get JWT secret from Secret Manager
                var authService = HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
                var jwtSecretName = Environment.GetEnvironmentVariable("JWT_SECRET_NAME") ?? "tpor-jwt-secret";
                jwtSecret = await authService.GetSecretAsync(jwtSecretName) ?? "default-secret-key";
            }
            else
            {
                // In development, get JWT secret from environment variable
                jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
                    ?? _authSettings.JwtSecret ?? "default-secret-key";
            }
            
            if (string.IsNullOrEmpty(jwtSecret))
            {
                _logger.LogError("JWT Secret is not configured");
                return StatusCode(500, "JWT Secret is not configured");
            }

            var token = GenerateJwtToken(request.Username, jwtSecret);
            var refreshToken = Guid.NewGuid().ToString();

            // Save refresh token to database (simplified for development)
            // In production, this would be saved to database

            return Ok(new
            {
                token,
                refreshToken,
                expiresIn = _authSettings.ExpirationMinutes * 60
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return StatusCode(500, "Internal server error occurred during login.");
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest("Refresh token is required.");
            }

            // Validate refresh token and get user
            var userId = await ValidateRefreshTokenAsync(request.RefreshToken);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid refresh token.");
            }

            // Get JWT secret for token generation
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
                ?? _authSettings.JwtSecret;
            var newToken = GenerateJwtToken(userId, jwtSecret);
            var newRefreshToken = Guid.NewGuid().ToString();

            // Save new refresh token (simplified for development)
            // In production, this would be saved to database

            return Ok(new
            {
                token = newToken,
                refreshToken = newRefreshToken,
                expiresIn = _authSettings.ExpirationMinutes * 60
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, "Internal server error occurred during token refresh.");
        }
    }

    private string GenerateJwtToken(string username, string jwtSecret)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtSecret);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, username)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_authSettings.ExpirationMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<string?> ValidateRefreshTokenAsync(string refreshToken)
    {
        // In a real application, you would validate the refresh token against the database
        // For this example, we'll return a simple validation
        return await Task.FromResult("admin");
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
