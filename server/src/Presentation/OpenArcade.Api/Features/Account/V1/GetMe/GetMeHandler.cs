using OpenArcade.Api.Common.Auth;
using OpenArcade.Domain.Publishers;

namespace OpenArcade.Api.Features.Account.V1.GetMe;

public sealed class GetMeHandler(IPublisherRepository publishers, IPublisherRequestRepository requests, CurrentUser current)
{
    public async Task<GetMeResponse> HandleAsync(CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var publisher = await publishers.GetByUserIdAsync(user.Id, ct);
        var latest = await requests.GetLatestForUserAsync(user.Id, ct);

        return new GetMeResponse(
            user.Id, user.Username, user.AvatarUrl, current.RoleNames, publisher?.IsActive == true, latest?.Status.ToString());
    }
}
