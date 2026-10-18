using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Community;

namespace OpenArcade.Api.Features.Comments.V1.DeleteComment;

public sealed class DeleteCommentHandler(ICommentRepository comments, IUnitOfWork uow, CurrentUser current)
{
    public async Task<Result> HandleAsync(Guid id, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var comment = await comments.GetAsync(id, ct);
        if (comment is null) return Error.NotFound();
        if (!comment.CanBeDeletedBy(user.Id, current.IsAdmin)) return Error.Forbidden();

        comment.Delete();
        comments.Update(comment);
        await uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
