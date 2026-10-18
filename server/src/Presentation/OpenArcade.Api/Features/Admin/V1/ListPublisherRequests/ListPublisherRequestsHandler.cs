using OpenArcade.Domain.Publishers;

namespace OpenArcade.Api.Features.Admin.V1.ListPublisherRequests;

public sealed class ListPublisherRequestsHandler(IPublisherRequestRepository requests)
{
    public async Task<IReadOnlyList<ListPublisherRequestsResponse>> HandleAsync(CancellationToken ct) =>
        (await requests.ListPendingAsync(ct))
            .Select(r => new ListPublisherRequestsResponse(r.Id, r.Username, r.StudioName, r.Links, r.CreatedAt))
            .ToList();
}
