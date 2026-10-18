using OpenArcade.Domain.Common;

namespace OpenArcade.Domain.Users;

public enum UserStatus { Active, Banned }

/// <summary>An OpenArcade account, linked to a Keycloak identity.</summary>
public sealed class User
{
    public Guid Id { get; internal set; }
    public string KeycloakId { get; internal set; } = "";
    public string Username { get; internal set; } = "";
    public string? Email { get; internal set; }
    public string? AvatarUrl { get; internal set; }
    public UserStatus Status { get; internal set; }
    public DateTimeOffset CreatedAt { get; internal set; }

    internal User() { }

    public static User Create(string keycloakId, string username, string? email, string? avatarUrl, DateTimeOffset now) => new()
    {
        Id = Guid.CreateVersion7(),
        KeycloakId = keycloakId,
        Username = username,
        Email = email,
        AvatarUrl = avatarUrl,
        Status = UserStatus.Active,
        CreatedAt = now,
    };

    public bool IsBanned => Status == UserStatus.Banned;

    public void Ban() => Status = UserStatus.Banned;
    public void Unban() => Status = UserStatus.Active;

    /// <summary>Banned accounts can't comment, vote or like.</summary>
    public void EnsureCanParticipate()
    {
        if (IsBanned) throw new DomainRuleException("Your account is banned from commenting and voting.");
    }
}
