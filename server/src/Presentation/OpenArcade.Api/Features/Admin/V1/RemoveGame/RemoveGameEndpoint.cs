using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Admin.V1.RemoveGame;

public sealed class RemoveGameEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/games/{id:guid}/archive", async (Guid id, RemoveGameHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, restore: false, ct)).ToHttp())
            .RequireAuthorization(Roles.Admin);

        app.MapPost("/admin/games/{id:guid}/restore", async (Guid id, RemoveGameHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, restore: true, ct)).ToHttp())
            .RequireAuthorization(Roles.Admin);
    }
}
