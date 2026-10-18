using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Admin.V1.BanUser;

public sealed class BanUserEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/users/{id:guid}/ban", async (Guid id, BanUserHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, ban: true, ct)).ToHttp())
            .RequireAuthorization(Roles.Admin);

        app.MapPost("/admin/users/{id:guid}/unban", async (Guid id, BanUserHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, ban: false, ct)).ToHttp())
            .RequireAuthorization(Roles.Admin);
    }
}
