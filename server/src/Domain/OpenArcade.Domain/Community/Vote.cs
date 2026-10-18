using OpenArcade.Domain.Common;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;
using OpenArcade.Domain.Users;

namespace OpenArcade.Domain.Community;

public enum VoteValue { Dislike = -1, Like = 1 }

/// <summary>A player's thumbs up or down on a game. One per player per game; it can be changed or removed.</summary>
public sealed class Vote
{
    public Guid UserId { get; internal set; }
    public Guid GameId { get; internal set; }
    public VoteValue Value { get; internal set; }
    public DateTimeOffset UpdatedAt { get; internal set; }

    internal Vote() { }

    /// <summary>Casts a vote. Banned players can't vote, and publishers can't vote on their own games.</summary>
    public static Vote Cast(User voter, Game game, Publisher publisher, VoteValue value, DateTimeOffset now)
    {
        voter.EnsureCanParticipate();
        if (!game.IsListed) throw new DomainRuleException("Only listed games can be voted on.");
        if (publisher.Id != game.PublisherId) throw new ArgumentException("Publisher doesn't own this game.", nameof(publisher));
        if (publisher.UserId == voter.Id) throw new DomainRuleException("You can't vote on your own game.");

        return new Vote { UserId = voter.Id, GameId = game.Id, Value = value, UpdatedAt = now };
    }

    public void Change(VoteValue value, DateTimeOffset now)
    {
        Value = value;
        UpdatedAt = now;
    }
}

public sealed record VoteCounts(int Likes, int Dislikes);

public interface IVoteRepository
{
    Task<Vote?> GetAsync(Guid userId, Guid gameId, CancellationToken ct = default);
    Task<VoteCounts> CountAsync(Guid gameId, CancellationToken ct = default);
    void Add(Vote vote);
    void Update(Vote vote);
    void Remove(Vote vote);
}
