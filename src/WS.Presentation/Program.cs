using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using WS.Infrastructure;
using WS.Presentation.Utilities;

var builder = WebApplication.CreateBuilder(args);

var dir    = Environment.GetEnvironmentVariable("ASPNETCORE_DIRECTORY");
var env    = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
             ?? throw new Exception("Set \"ASPNETCORE_ENVIRONMENT\"");

builder.Configuration
       .SetBasePath(dir ?? Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
       .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
       .AddEnvironmentVariables();

// 認證
builder.Services.AddAuthentication().Jwt(builder.Configuration);

// 授權
builder.Services.AddAuthorization(o => {
    o.Jwt();
});

// Services
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor   |
                       ForwardedHeaders.XForwardedHost  | 
                       ForwardedHeaders.XForwardedProto, 
        
    KnownProxies = { IPAddress.Parse("127.0.0.1") }
});

app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();


app.MapGet("/ws", async (HttpContext context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        await context.EstablishAConnectionAsync();
    }
}).RequireAuthorization();


app.Run();
