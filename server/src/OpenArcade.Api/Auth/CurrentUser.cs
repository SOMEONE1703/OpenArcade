using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OpenArcade.Api.Data;

namespace OpenArcade.Api.Auth;

/// <summary>
/// Maps the signed-in Keycloak identity to a row in <c>users</c>, creating it on first sight.
/// </summary>
public class CurrentUser(IHttpContextAccessor http, AppDbContext db)
{
    private User? _user;

    public ClaimsPrincipal Principal => http.HttpContext?.User ?? new ClaimsPrincipal();
    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;
    public bool IsAdmin => Principal.IsInRole(Roles.Admin);

    public async Task<User?> GetAsync(CancellationToken ct = default)
    {
        if (!IsAuthenticated) return null;
        if (_user is not null) return _user;

        var sub = Principal.FindFirstValue("sub");
        if (sub is null) return null;

        _user = await db.Users.FirstOrDefaultAsync(u => u.KeycloakId == sub, ct);
        if (_user is null)
        {
            _user = new User
            {
                KeycloakId = sub,
                Username = await UniqueUsernameAsync(Principal.FindFirstValue("preferred_username") ?? "player", ct),
                Email = Principal.FindFirstValue("email"),
                AvatarUrl = Principal.FindFirstValue("picture"),
            };
            db.Users.Add(_user);
            await db.SaveChangesAsync(ct);
        }
        return _user;
    }

    /// <summary>Returns the user, or throws if the request is anonymous. Use on [Authorize] endpoints.</summary>
    public async Task<User> RequireAsync(CancellationToken ct = default) =>
        await GetAsync(ct) ?? throw new UnauthorizedAccessException();

    private async Task<string> UniqueUsernameAsync(string wanted, CancellationToken ct)
    {
        var name = wanted;
        for (var i = 2; await db.Users.AnyAsync(u => u.Username == name, ct); i++)
            name = $"{wanted}{i}";
        return name;
    }
}
