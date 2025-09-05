using Microsoft.AspNetCore.Authorization;

namespace WS.Presentation.Utilities;

public static class AuthorizationExtension
{
    /// <summary>
    /// Json Web Token 授權
    /// </summary>
    /// <param name="options"></param>
    public static void Jwt(this AuthorizationOptions options)
    {
        options.AddPolicy("jwt", builder =>
        {
            builder.RequireAuthenticatedUser()
                   .AddAuthenticationSchemes("jwt")
                   .RequireClaim("sub")
                   .Build();
        });
    }
}