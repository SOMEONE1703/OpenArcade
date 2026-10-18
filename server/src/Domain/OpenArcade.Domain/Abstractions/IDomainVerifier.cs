namespace OpenArcade.Domain.Abstractions;

/// <summary>Proves a publisher controls a game's domain (DNS TXT record or /.well-known/openarcade.txt).</summary>
public interface IDomainVerifier
{
    Task<bool> VerifyAsync(string playUrl, string token, CancellationToken ct = default);
}
