using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Admin.V1.GetAdminGame;

public sealed class GetAdminGameEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/admin/games/{id:guid}", async (Guid id, GetAdminGameHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, ct)).ToHttp())
            .RequireAuthorization(Roles.Admin);
}
