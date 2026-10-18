using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Admin.V1.GetAdminGame;

public sealed class GetAdminGameHandler(IGameRepository games)
{
    public async Task<Result<AdminGameResponse>> HandleAsync(Guid id, CancellationToken ct) =>
        await games.GetAsync(id, ct) is { } game ? AdminGameResponse.From(game) : Error.NotFound();
}
