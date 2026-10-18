using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Games.V1.VoteOnGame;

public sealed class VoteOnGameEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/games/{slug}/vote", async (string slug, VoteOnGameRequest request, VoteOnGameHandler handler, CancellationToken ct) =>
                (await handler.SetAsync(slug, request, ct)).ToHttp())
            .RequireAuthorization();

        app.MapDelete("/games/{slug}/vote", async (string slug, VoteOnGameHandler handler, CancellationToken ct) =>
                (await handler.ClearAsync(slug, ct)).ToHttp())
            .RequireAuthorization();
    }
}
