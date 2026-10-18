using System.Text.RegularExpressions;
using OpenArcade.Domain.Common;

namespace OpenArcade.Domain.Games;

public enum GameStatus { Draft, InReview, Listed, Hidden, Delisted }
public enum MediaKind { Thumbnail, Screenshot, Video }

public sealed record GameMedia(MediaKind Kind, string Url, int SortOrder);

/// <summary>What a publisher fills in when registering or editing a game.</summary>
public sealed record GameListing(
    string Slug, string Title, string Description, IReadOnlyList<string> Tags, string PlayUrl,
    bool MobileFriendly, string? ThumbnailUrl, IReadOnlyList<string> Screenshots);

/// <summary>
/// A self-hosted game in the catalog. OpenArcade stores its listing and status; the game itself runs on the
/// publisher's domain.
/// </summary>
public sealed partial class Game
{
    /// <summary>Failed daily health checks in a row before a listed game is hidden.</summary>
    public const int HealthFailuresBeforeHiding = 3;

    public Guid Id { get; internal set; }
    public Guid PublisherId { get; internal set; }
    public string Slug { get; internal set; } = "";
    public string Title { get; internal set; } = "";
    public string Description { get; internal set; } = "";
    public IReadOnlyList<string> Tags { get; internal set; } = [];
    public string PlayUrl { get; internal set; } = "";
    public string Domain { get; internal set; } = "";
    /// <summary>Token the publisher puts in DNS TXT or /.well-known/openarcade.txt.</summary>
    public string VerificationToken { get; internal set; } = "";
    public DateTimeOffset? DomainVerifiedAt { get; internal set; }
    public GameStatus Status { get; internal set; }
    public bool MobileFriendly { get; internal set; }
    public int ConsecutiveHealthFailures { get; internal set; }
    public IReadOnlyList<GameMedia> Media { get; internal set; } = [];
    public DateTimeOffset CreatedAt { get; internal set; }

    // Cached counts, refreshed by the stats job and after each vote.
    public int LikeCount { get; internal set; }
    public int DislikeCount { get; internal set; }
    public int PlayCount { get; internal set; }
    public int PlayCount30d { get; internal set; }
    public double TrendingScore { get; internal set; }

    internal Game() { }

    public bool IsListed => Status == GameStatus.Listed;
    public bool IsDomainVerified => DomainVerifiedAt is not null;
    public string? ThumbnailUrl => Media.FirstOrDefault(m => m.Kind == MediaKind.Thumbnail)?.Url;

    public static Game Register(Guid publisherId, GameListing listing, string verificationToken, DateTimeOffset now)
    {
        var game = new Game
        {
            Id = Guid.CreateVersion7(),
            PublisherId = publisherId,
            VerificationToken = verificationToken,
            Status = GameStatus.Draft,
            CreatedAt = now,
        };
        game.Apply(listing);
        return game;
    }

    /// <summary>Edits the listing. Changing the play URL or domain sends a listed game back to review.</summary>
    public void UpdateListing(GameListing listing)
    {
        var oldUrl = PlayUrl;
        var oldDomain = Domain;
        Apply(listing);

        if (PlayUrl == oldUrl && Domain == oldDomain) return;
        if (Domain != oldDomain) DomainVerifiedAt = null;
        if (Status is GameStatus.Listed or GameStatus.Hidden) Status = GameStatus.InReview;
    }

    public void MarkDomainVerified(DateTimeOffset now) => DomainVerifiedAt = now;

    public void SubmitForReview()
    {
        if (Status is not (GameStatus.Draft or GameStatus.Hidden)) throw new DomainRuleException($"The game is {Status}.");
        if (!IsDomainVerified) throw new DomainRuleException("Verify the domain first.");
        Status = GameStatus.InReview;
    }

    public void Approve()
    {
        if (Status is not (GameStatus.InReview or GameStatus.Hidden)) throw new DomainRuleException($"The game is {Status}.");
        Status = GameStatus.Listed;
        ConsecutiveHealthFailures = 0;
    }

    public void SendBackToDraft()
    {
        if (Status != GameStatus.InReview) throw new DomainRuleException($"The game is {Status}.");
        Status = GameStatus.Draft;
    }

    /// <summary>Kill switch: removes the play page immediately.</summary>
    public void Delist() => Status = GameStatus.Delisted;

    /// <summary>
    /// Applies a daily health check. Three failures in a row hide a listed game; a hidden game that passes again
    /// (and still has a verified domain) comes back.
    /// </summary>
    /// <returns>True when this check hid the game.</returns>
    public bool RecordHealthCheck(bool passed)
    {
        if (passed)
        {
            ConsecutiveHealthFailures = 0;
            if (Status == GameStatus.Hidden && IsDomainVerified) Status = GameStatus.Listed;
            return false;
        }

        ConsecutiveHealthFailures++;
        if (ConsecutiveHealthFailures < HealthFailuresBeforeHiding || Status != GameStatus.Listed) return false;
        Status = GameStatus.Hidden;
        return true;
    }

    /// <summary>The verification record is gone (expired or sold domain), so the game is hidden.</summary>
    public void LoseDomainVerification()
    {
        DomainVerifiedAt = null;
        if (Status == GameStatus.Listed) Status = GameStatus.Hidden;
    }

    public void SetVoteCounts(int likes, int dislikes)
    {
        LikeCount = likes;
        DislikeCount = dislikes;
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();

    private void Apply(GameListing listing)
    {
        if (!SlugPattern().IsMatch(listing.Slug ?? "")) throw new DomainRuleException("Slug must be lowercase letters, numbers and dashes.");
        if (string.IsNullOrWhiteSpace(listing.Title)) throw new DomainRuleException("Title is required.");
        if (!Uri.TryCreate(listing.PlayUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http"))
            throw new DomainRuleException("Play URL must be an absolute http(s) URL.");

        Slug = listing.Slug!;
        Title = listing.Title.Trim();
        Description = listing.Description?.Trim() ?? "";
        Tags = listing.Tags.Select(t => t.Trim().ToLowerInvariant()).Where(t => t.Length > 0).Distinct().ToList();
        PlayUrl = listing.PlayUrl;
        Domain = uri.Host;
        MobileFriendly = listing.MobileFriendly;

        var media = new List<GameMedia>();
        if (!string.IsNullOrWhiteSpace(listing.ThumbnailUrl)) media.Add(new GameMedia(MediaKind.Thumbnail, listing.ThumbnailUrl, 0));
        media.AddRange(listing.Screenshots.Select((url, i) => new GameMedia(MediaKind.Screenshot, url, i + 1)));
        Media = media;
    }
}
