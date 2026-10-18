using System.Security.Claims;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Users;

namespace OpenArcade.Api.Common.Auth;

/// <summary>
/// Maps the signed-in Keycloak identity to an OpenArcade <see cref="User"/>, creating the account on first sight.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor http, IUserRepository users, IUnitOfWork uow, IClock clock)
{
    private User? _user;

    public ClaimsPrincipal Principal => http.HttpContext?.User ?? new ClaimsPrincipal();
    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;
    public bool IsAdmin => Principal.IsInRole(Roles.Admin);

    /// <summary>OpenArcade realm roles in the token (player, publisher, admin).</summary>
    public IReadOnlyList<string> RoleNames => Principal.FindAll(ClaimTypes.Role).Select(c => c.Value)
        .Where(r => r is Roles.Player or Roles.Publisher or Roles.Admin).Distinct().ToList();

    public async Task<User?> GetAsync(CancellationToken ct = default)
    {
        if (!IsAuthenticated) return null;
        if (_user is not null) return _user;

        var sub = Principal.FindFirstValue("sub");
        if (sub is null) return null;

        _user = await users.GetByKeycloakIdAsync(sub, ct);
        if (_user is null)
        {
            var username = await UniqueUsernameAsync(Principal.FindFirstValue("preferred_username") ?? "player", ct);
            _user = User.Create(sub, username, Principal.FindFirstValue("email"), Principal.FindFirstValue("picture"), clock.UtcNow);
            users.Add(_user);
            await uow.SaveChangesAsync(ct);
        }
        return _user;
    }

    /// <summary>Returns the user on endpoints that require authorization.</summary>
    public async Task<User> RequireAsync(CancellationToken ct = default) =>
        await GetAsync(ct) ?? throw new UnauthorizedAccessException();

    private async Task<string> UniqueUsernameAsync(string wanted, CancellationToken ct)
    {
        var name = wanted;
        for (var i = 2; await users.UsernameExistsAsync(name, ct); i++)
            name = $"{wanted}{i}";
        return name;
    }
}
