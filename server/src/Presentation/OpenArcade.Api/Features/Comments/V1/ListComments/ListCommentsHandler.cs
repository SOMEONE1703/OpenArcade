using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Community;
using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Comments.V1.ListComments;

public sealed class ListCommentsHandler(IGameRepository games, ICommentRepository comments, CurrentUser current)
{
    private const int PageSize = 30;

    public async Task<Result<ListCommentsResponse>> HandleAsync(string slug, ListCommentsRequest request, CancellationToken ct)
    {
        var game = await games.GetListedBySlugAsync(slug, ct);
        if (game is null) return Error.NotFound();

        var viewer = await current.GetAsync(ct);
        var sort = request.Sort == "top" ? CommentSort.Top : CommentSort.New;
        var items = await comments.ListTopLevelAsync(game.Id, sort, Math.Max(request.Skip ?? 0, 0), PageSize, viewer?.Id, ct);
        var hidden = await comments.CountHiddenTopLevelAsync(game.Id, ct);

        return new ListCommentsResponse(items.Select(CommentResponse.From).ToList(), hidden);
    }
}
