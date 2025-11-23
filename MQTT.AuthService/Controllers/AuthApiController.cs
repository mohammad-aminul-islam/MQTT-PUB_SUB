using Microsoft.AspNetCore.Mvc;
using MQTT.AuthService.DTOs;

namespace MQTT.AuthService.Controllers;

[ApiController]
[Route("api/v1")]
public class AuthApiController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<AuthApiController> _logger;

    public AuthApiController(ILogger<AuthApiController> logger)
    {
        _logger = logger;
    }

    [HttpPost("auth")]
    public async Task<IActionResult> Authenticate([FromForm] AuthCreateDto authCreateDto)
    {
        // Simulate authentication logic
        if (authCreateDto.UserName == "123" && authCreateDto.Password == "123")
        {
            return Ok(new { allow = true });
        }
        return Unauthorized(new { allow = false });
    }
}
