using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Games.V1.ListTags;

public sealed class ListTagsHandler(IGameRepository games)
{
    /// <summary>Tags used by at least one listed game, alphabetically.</summary>
    public Task<IReadOnlyList<string>> HandleAsync(CancellationToken ct) => games.ListTagsAsync(ct);
}
