using Microsoft.AspNetCore.Mvc;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[Route("api")]
public sealed class HealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "healthy" });
    }
}
