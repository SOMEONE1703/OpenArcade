using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Admin.V1.ReviewGame;

public sealed class ReviewGameEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/games/{id:guid}/approve", async (Guid id, ReviewGameHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, approve: true, ct)).ToHttp())
            .RequireAuthorization(Roles.Admin);

        app.MapPost("/admin/games/{id:guid}/reject", async (Guid id, ReviewGameHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, approve: false, ct)).ToHttp())
            .RequireAuthorization(Roles.Admin);
    }
}
