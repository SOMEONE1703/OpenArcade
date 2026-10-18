using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Account.V1.GetProfile;

public sealed class GetProfileEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/users/{username}", async (string username, GetProfileHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(username, ct)).ToHttp());
}
