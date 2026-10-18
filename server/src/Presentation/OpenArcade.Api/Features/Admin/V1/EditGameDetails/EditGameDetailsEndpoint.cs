using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Admin.V1.EditGameDetails;

public sealed class EditGameDetailsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/admin/games/{id:guid}", async (Guid id, EditGameDetailsRequest request, EditGameDetailsHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, request, ct)).ToHttp())
            .RequireAuthorization(Roles.Admin);
}
