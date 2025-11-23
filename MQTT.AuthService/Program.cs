using Microsoft.AspNetCore.WebSockets;
using MQTT.AuthService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<DeviceAuthService>();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddWebSockets(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(120);
});

builder.Services.AddReverseProxy()
    .LoadFromMemory(new[]
    {
        new Yarp.ReverseProxy.Configuration.RouteConfig
        {
            RouteId = "mqtt_ws",
            ClusterId = "mosquitto",
            Match = new Yarp.ReverseProxy.Configuration.RouteMatch
            {
                Path = "/mqtt/{**catch-all}"
            },
            Transforms = new[]
            {
                new Dictionary<string, string>
                {
                    ["RequestHeaderOriginalHost"] = "true"
                }
            }
        }
    },
    new[]
    {
        new Yarp.ReverseProxy.Configuration.ClusterConfig
        {
            ClusterId = "mosquitto",
            Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
            {
                { "destination1", new Yarp.ReverseProxy.Configuration.DestinationConfig { Address = "ws://localhost:9001" } }
            }
        }
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
// WebSocket upgrade + authentication middleware
app.Use(async (context, next) =>
{
    // Authenticate device (JWT / API Key)
    var token = context.Request.Headers["Authorization"].ToString();
    var service = context.RequestServices.GetRequiredService<DeviceAuthService>();
    if (!service.IsValidToken(token, out string tenantId, out string deviceId))
    {
        context.Response.StatusCode = 401;
        return;
    }

    // Optional: add tenant prefix to topic via header or MQTT payload transform
    context.Request.Headers.Add("X-Tenant-Id", tenantId);

    await next();
});

app.MapReverseProxy();

app.UseAuthorization();

app.MapControllers();

app.Run();

