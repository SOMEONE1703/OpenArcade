namespace OpenArcade.Domain.Games;

public enum CatalogSort { New, MostPlayed, MostPlayed30d, Trending, Top }

public sealed record CatalogQuery(CatalogSort Sort, string? Tag, string? Search, int Limit);

/// <summary>A listed game as it appears on catalog shelves and profile pages.</summary>
public sealed record GameCard(
    Guid Id, string Slug, string Title, string? ThumbnailUrl, IReadOnlyList<string> Tags, string Studio,
    int LikeCount, int DislikeCount, int PlayCount, int PlayCount30d, bool MobileFriendly);

/// <summary>A game waiting for an admin to play and review it.</summary>
public sealed record ReviewQueueItem(
    Guid Id, string Slug, string Title, string PlayUrl, string Domain, string Studio, DateTimeOffset? DomainVerifiedAt);

/// <summary>Filters for the admin's list of every game. Archived games only show when asked for by status.</summary>
public sealed record AdminGameQuery(GameStatus? Status, string? Search, int Limit);

/// <summary>A game of any status, as an admin sees it in the games list.</summary>
public sealed record AdminGameRow(
    Guid Id, string Slug, string Title, string? ThumbnailUrl, string Studio, GameStatus Status, string PlayUrl,
    string Domain, bool DomainVerified, int PlayCount, int LikeCount, int DislikeCount, int ConsecutiveHealthFailures,
    DateTimeOffset CreatedAt);

public interface IGameRepository
{
    Task<Game?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Game?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Game?> GetListedBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Game>> ListByPublisherAsync(Guid publisherId, CancellationToken ct = default);
    Task<IReadOnlyList<Game>> ListByStatusAsync(IReadOnlyCollection<GameStatus> statuses, CancellationToken ct = default);
    void Add(Game game);
    void Update(Game game);

    Task<IReadOnlyList<GameCard>> ListCatalogAsync(CatalogQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListTagsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<GameCard>> ListFavoritesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<ReviewQueueItem>> ListReviewQueueAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AdminGameRow>> ListForAdminAsync(AdminGameQuery query, CancellationToken ct = default);
}

public interface IPlayRepository
{
    /// <summary>Records the play and bumps the game's play count, unless this visitor already played today.</summary>
    /// <returns>True if the play was counted.</returns>
    Task<bool> TryRecordAsync(Play play, CancellationToken ct = default);
}

public interface IGameCheckRepository
{
    void Add(GameCheck check);
}
