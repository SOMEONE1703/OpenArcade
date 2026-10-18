using OpenArcade.Api.Features.Games.V1;

namespace OpenArcade.Api.Features.Account.V1.GetProfile;

public sealed record GetProfileResponse(string Username, string? AvatarUrl, DateTimeOffset JoinedAt, IReadOnlyList<GameCardResponse> Favorites);
