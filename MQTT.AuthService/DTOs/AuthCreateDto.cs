using Microsoft.AspNetCore.Mvc;

namespace MQTT.AuthService.DTOs;

public class AuthCreateDto
{
    [FromForm(Name = "username")]
    public string UserName { get; set; }
    [FromForm(Name = "password")]
    public string Password { get; set; }
}

