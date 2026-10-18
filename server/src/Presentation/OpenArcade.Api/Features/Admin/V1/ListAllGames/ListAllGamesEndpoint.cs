using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Admin.V1.ListAllGames;

public sealed class ListAllGamesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/admin/games", async ([AsParameters] ListAllGamesRequest request, ListAllGamesHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(request, ct)).ToHttp())
            .RequireAuthorization(Roles.Admin);
}
