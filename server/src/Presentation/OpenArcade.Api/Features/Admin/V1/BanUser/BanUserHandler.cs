using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Users;

namespace OpenArcade.Api.Features.Admin.V1.BanUser;

/// <summary>Bans or unbans an account from commenting, voting and liking.</summary>
public sealed class BanUserHandler(IUserRepository users, IUnitOfWork uow)
{
    public async Task<Result> HandleAsync(Guid id, bool ban, CancellationToken ct)
    {
        var user = await users.GetAsync(id, ct);
        if (user is null) return Error.NotFound();

        if (ban) user.Ban();
        else user.Unban();
        users.Update(user);
        await uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
