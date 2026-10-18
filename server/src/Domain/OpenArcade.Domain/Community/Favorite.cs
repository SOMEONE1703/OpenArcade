namespace OpenArcade.Domain.Community;

/// <summary>A game a player saved to their favorites list.</summary>
public sealed record Favorite(Guid UserId, Guid GameId, DateTimeOffset CreatedAt);

public interface IFavoriteRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken ct = default);
    void Add(Favorite favorite);
    Task RemoveAsync(Guid userId, Guid gameId, CancellationToken ct = default);
}
