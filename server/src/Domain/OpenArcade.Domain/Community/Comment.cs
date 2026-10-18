using OpenArcade.Domain.Common;
using OpenArcade.Domain.Users;

namespace OpenArcade.Domain.Community;

public enum CommentStatus { Visible, Hidden, Deleted }

/// <summary>
/// An Instagram-style comment on a game. Replies are one level deep; publishers can pin one comment and hide
/// others on their own game, which only collapses them.
/// </summary>
public sealed class Comment
{
    public const int MaxLength = 2000;

    public Guid Id { get; internal set; }
    public Guid GameId { get; internal set; }
    public Guid UserId { get; internal set; }
    public Guid? ParentId { get; internal set; }
    public string Body { get; internal set; } = "";
    public int LikeCount { get; internal set; }
    public bool Pinned { get; internal set; }
    public CommentStatus Status { get; internal set; }
    public DateTimeOffset CreatedAt { get; internal set; }

    internal Comment() { }

    public bool IsTopLevel => ParentId is null;

    /// <param name="parent">The comment being replied to, if any. Replying to a reply attaches to its parent.</param>
    public static Comment Post(User author, Guid gameId, Comment? parent, string body, DateTimeOffset now)
    {
        author.EnsureCanParticipate();
        var text = body?.Trim() ?? "";
        if (text.Length is 0 or > MaxLength) throw new DomainRuleException($"Comments must be 1 to {MaxLength} characters.");
        if (parent is not null && parent.GameId != gameId) throw new DomainRuleException("Parent comment not found.");

        return new Comment
        {
            Id = Guid.CreateVersion7(),
            GameId = gameId,
            UserId = author.Id,
            ParentId = parent is null ? null : parent.ParentId ?? parent.Id,
            Body = text,
            Status = CommentStatus.Visible,
            CreatedAt = now,
        };
    }

    public bool CanBeDeletedBy(Guid userId, bool isAdmin) => isAdmin || UserId == userId;

    public void Delete()
    {
        Status = CommentStatus.Deleted;
        Pinned = false;
    }

    public void Hide()
    {
        if (Status == CommentStatus.Deleted) throw new DomainRuleException("The comment was deleted.");
        Status = CommentStatus.Hidden;
        Pinned = false;
    }

    public void Unhide()
    {
        if (Status == CommentStatus.Deleted) throw new DomainRuleException("The comment was deleted.");
        Status = CommentStatus.Visible;
    }

    /// <summary>Pins or unpins. The caller unpins the game's other comments first, so only one stays pinned.</summary>
    public void SetPinned(bool pinned)
    {
        if (pinned && !IsTopLevel) throw new DomainRuleException("Only top-level comments can be pinned.");
        if (pinned && Status != CommentStatus.Visible) throw new DomainRuleException("Only visible comments can be pinned.");
        Pinned = pinned;
    }
}

public enum CommentSort { New, Top }

/// <summary>A visible comment with what the viewer needs to render it.</summary>
public sealed record CommentView(
    Guid Id, Guid? ParentId, string Username, string? AvatarUrl, string Body, int LikeCount,
    bool Pinned, bool LikedByViewer, bool ByViewer, int ReplyCount, DateTimeOffset CreatedAt);

public interface ICommentRepository
{
    Task<Comment?> GetAsync(Guid id, CancellationToken ct = default);
    void Add(Comment comment);
    void Update(Comment comment);
    Task UnpinAllAsync(Guid gameId, CancellationToken ct = default);

    /// <summary>Adds a like and bumps the count. Returns false if the user already liked it.</summary>
    Task<bool> AddLikeAsync(Guid userId, Guid commentId, CancellationToken ct = default);
    /// <summary>Removes a like and lowers the count. Returns false if there was no like.</summary>
    Task<bool> RemoveLikeAsync(Guid userId, Guid commentId, CancellationToken ct = default);

    Task<IReadOnlyList<CommentView>> ListTopLevelAsync(Guid gameId, CommentSort sort, int skip, int take, Guid? viewerId, CancellationToken ct = default);
    Task<IReadOnlyList<CommentView>> ListRepliesAsync(Guid parentId, Guid? viewerId, CancellationToken ct = default);
    Task<int> CountHiddenTopLevelAsync(Guid gameId, CancellationToken ct = default);
}
