using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace WS.Presentation.Utilities;

public static class AuthenticationExtension
{
    /// <summary>
    /// Json Web Token 身份認證
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static AuthenticationBuilder Jwt(
        this AuthenticationBuilder builder,
        IConfiguration config)
    {
        builder.AddJwtBearer("jwt", o =>
        {
            // Description - 
            //     告訴 framework，不要把 claim type 變成 Microsoft 自定義的 Type 
            o.MapInboundClaims = false;
        
            // Description - 
            //     定義 openid 的 endpoint
            o.Authority = $"{config["OIDC"]}/oauth";
            // TODO: 這個很危險，但去掉不知道為啥會有問題，去搞清楚，並去掉它
            //o.RequireHttpsMetadata = false;
            
            // Description - 
            //     定義 Validate 過程中要 validate 哪些資料
            o.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
            
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireExpirationTime = true
            };
            
            // 啟用詳細錯誤訊息
            o.IncludeErrorDetails = true;
        
            // 事件處理器用於記錄詳細錯誤
            o.Events = new JwtBearerEvents
            {
                // 當認證失敗時
                OnChallenge = async context =>
                {
                    context.HandleResponse();
        
                    var problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "Unauthorized",
                        Detail = context.ErrorDescription ?? "无效的认证令牌",
                        Instance = context.Request.Path
                    };
                
                    var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                    problemDetails.Extensions["traceId"] = traceId;
        
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/problem+json";
        
                    await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
                },
            };
        });

        return builder;
    }
}