using Microsoft.AspNetCore.Mvc;

namespace TPOR.Intranet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            // API service health check (no database dependency)
            var healthStatus = new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                services = new
                {
                    api = "Healthy",
                    note = "Database operations are handled by Worker service"
                }
            };

            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new
            {
                status = "Unhealthy",
                timestamp = DateTime.UtcNow,
                error = ex.Message
            });
        }
    }
}
