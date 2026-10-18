using OpenArcade.Api.Common.Results;
using OpenArcade.Api.Features.Games.V1;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Users;

namespace OpenArcade.Api.Features.Account.V1.GetProfile;

public sealed class GetProfileHandler(IUserRepository users, IGameRepository games)
{
    /// <summary>A player's public profile: who they are and their favorite listed games.</summary>
    public async Task<Result<GetProfileResponse>> HandleAsync(string username, CancellationToken ct)
    {
        var user = await users.GetByUsernameAsync(username, ct);
        if (user is null) return Error.NotFound();

        var favorites = await games.ListFavoritesAsync(user.Id, ct);
        return new GetProfileResponse(user.Username, user.AvatarUrl, user.CreatedAt, favorites.Select(GameCardResponse.From).ToList());
    }
}
