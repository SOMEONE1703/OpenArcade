using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Publishers;

namespace OpenArcade.Api.Features.Account.V1.ApplyAsPublisher;

public sealed class ApplyAsPublisherHandler(
    IPublisherRepository publishers, IPublisherRequestRepository requests, IUnitOfWork uow, CurrentUser current, IClock clock)
{
    public async Task<Result> HandleAsync(ApplyAsPublisherRequest request, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        if (await publishers.GetByUserIdAsync(user.Id, ct) is not null) return Error.Conflict("You are already a publisher.");
        if ((await requests.GetLatestForUserAsync(user.Id, ct))?.Status == RequestStatus.Pending)
            return Error.Conflict("You already have a pending request.");

        requests.Add(PublisherRequest.Submit(user.Id, request.StudioName, request.Links, clock.UtcNow));
        await uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
