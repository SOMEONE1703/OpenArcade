using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Comments.V1.PostComment;

public sealed class PostCommentEndpoint : IEndpoint
{
    /// <summary>Rate limit policy: 5 comments per minute per signed-in user.</summary>
    public const string RateLimitPolicy = "comment-post";

    public void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/games/{slug}/comments", async (string slug, PostCommentRequest request, PostCommentHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(slug, request, ct)).ToHttp(c => TypedResults.Created($"/api/v1/comments/{c.Id}", c)))
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicy);
}
