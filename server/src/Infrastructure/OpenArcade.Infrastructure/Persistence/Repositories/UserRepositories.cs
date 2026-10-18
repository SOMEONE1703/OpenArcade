using Microsoft.EntityFrameworkCore;
using OpenArcade.Domain.Publishers;
using OpenArcade.Domain.Users;
using OpenArcade.Infrastructure.Persistence.Mappers;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetAsync(Guid id, CancellationToken ct) =>
        (await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct))?.ToDomain();

    public async Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken ct) =>
        (await db.Users.FirstOrDefaultAsync(u => u.KeycloakId == keycloakId, ct))?.ToDomain();

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct) =>
        (await db.Users.FirstOrDefaultAsync(u => u.Username == username, ct))?.ToDomain();

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Username == username, ct);

    public void Add(User user)
    {
        var record = new UserRecord();
        user.CopyTo(record);
        db.Users.Add(record);
    }

    public void Update(User user) => user.CopyTo(Tracked.Find(db.Users, r => r.Id == user.Id));
}

internal sealed class PublisherRepository(AppDbContext db) : IPublisherRepository
{
    public async Task<Publisher?> GetAsync(Guid id, CancellationToken ct) =>
        (await db.Publishers.FirstOrDefaultAsync(p => p.Id == id, ct))?.ToDomain();

    public async Task<Publisher?> GetByUserIdAsync(Guid userId, CancellationToken ct) =>
        (await db.Publishers.FirstOrDefaultAsync(p => p.UserId == userId, ct))?.ToDomain();

    public void Add(Publisher publisher)
    {
        var record = new PublisherRecord();
        publisher.CopyTo(record);
        db.Publishers.Add(record);
    }
}

internal sealed class PublisherRequestRepository(AppDbContext db) : IPublisherRequestRepository
{
    public async Task<PublisherRequest?> GetAsync(Guid id, CancellationToken ct) =>
        (await db.PublisherRequests.FirstOrDefaultAsync(r => r.Id == id, ct))?.ToDomain();

    public async Task<PublisherRequest?> GetLatestForUserAsync(Guid userId, CancellationToken ct) =>
        (await db.PublisherRequests.Where(r => r.UserId == userId).OrderByDescending(r => r.CreatedAt).FirstOrDefaultAsync(ct))?.ToDomain();

    public async Task<IReadOnlyList<PublisherRequestView>> ListPendingAsync(CancellationToken ct) =>
        await db.PublisherRequests.AsNoTracking()
            .Where(r => r.Status == RequestStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new PublisherRequestView(r.Id, r.User.Username, r.StudioName, r.Links, r.CreatedAt))
            .ToListAsync(ct);

    public void Add(PublisherRequest request)
    {
        var record = new PublisherRequestRecord();
        request.CopyTo(record);
        db.PublisherRequests.Add(record);
    }

    public void Update(PublisherRequest request) => request.CopyTo(Tracked.Find(db.PublisherRequests, r => r.Id == request.Id));
}
