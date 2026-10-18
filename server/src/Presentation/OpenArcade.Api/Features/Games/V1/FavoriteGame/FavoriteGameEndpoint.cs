using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Games.V1.FavoriteGame;

public sealed class FavoriteGameEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/games/{slug}/favorite", async (string slug, FavoriteGameHandler handler, CancellationToken ct) =>
                (await handler.AddAsync(slug, ct)).ToHttp())
            .RequireAuthorization();

        app.MapDelete("/games/{slug}/favorite", async (string slug, FavoriteGameHandler handler, CancellationToken ct) =>
                (await handler.RemoveAsync(slug, ct)).ToHttp())
            .RequireAuthorization();
    }
}
