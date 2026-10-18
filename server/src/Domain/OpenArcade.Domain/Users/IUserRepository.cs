namespace OpenArcade.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
    void Add(User user);
    void Update(User user);
}
