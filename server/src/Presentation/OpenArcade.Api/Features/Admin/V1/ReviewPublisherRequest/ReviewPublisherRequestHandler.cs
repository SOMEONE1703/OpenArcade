using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Publishers;

namespace OpenArcade.Api.Features.Admin.V1.ReviewPublisherRequest;

public sealed class ReviewPublisherRequestHandler(
    IPublisherRequestRepository requests, IPublisherRepository publishers, IUnitOfWork uow, CurrentUser current)
{
    public async Task<Result> HandleAsync(Guid id, bool approve, CancellationToken ct)
    {
        var admin = await current.RequireAsync(ct);
        var request = await requests.GetAsync(id, ct);
        if (request is null) return Error.NotFound();

        if (approve)
        {
            // TODO: also grant the "publisher" realm role in Keycloak via its admin API. Until then the
            // publishers table is what the publishing endpoints check.
            publishers.Add(request.Approve(admin.Id));
        }
        else
        {
            request.Reject(admin.Id);
        }
        requests.Update(request);
        await uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
