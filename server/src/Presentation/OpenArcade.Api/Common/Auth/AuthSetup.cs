using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace OpenArcade.Api.Common.Auth;

public static class Roles
{
    public const string Player = "player";
    public const string Publisher = "publisher";
    public const string Admin = "admin";
}

public static class AuthSetup
{
    public static IServiceCollection AddKeycloakAuth(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var section = config.GetSection("Keycloak");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.Authority = section["Authority"];
                o.Audience = section["Audience"];
                o.RequireHttpsMetadata = !env.IsDevelopment();
                o.MapInboundClaims = false;
                o.TokenValidationParameters.NameClaimType = "preferred_username";
                o.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
                o.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        if (ctx.Principal?.Identity is ClaimsIdentity id)
                            AddRealmRoles(id);
                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Roles.Publisher, p => p.RequireRole(Roles.Publisher, Roles.Admin))
            .AddPolicy(Roles.Admin, p => p.RequireRole(Roles.Admin));

        return services;
    }

    /// <summary>Keycloak puts realm roles in realm_access.roles; flatten them into role claims.</summary>
    private static void AddRealmRoles(ClaimsIdentity id)
    {
        var realmAccess = id.FindFirst("realm_access")?.Value;
        if (realmAccess is null) return;

        using var doc = JsonDocument.Parse(realmAccess);
        if (!doc.RootElement.TryGetProperty("roles", out var roles)) return;

        foreach (var role in roles.EnumerateArray())
        {
            var name = role.GetString();
            if (name is not null) id.AddClaim(new Claim(ClaimTypes.Role, name));
        }
    }
}
