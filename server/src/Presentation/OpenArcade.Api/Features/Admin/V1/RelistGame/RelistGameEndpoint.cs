using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Admin.V1.RelistGame;

public sealed class RelistGameEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/admin/games/{id:guid}/relist", async (Guid id, RelistGameHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, ct)).ToHttp())
            .RequireAuthorization(Roles.Admin);
}
