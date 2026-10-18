using DnsClient;
using OpenArcade.Domain.Abstractions;

namespace OpenArcade.Infrastructure.Checks;

/// <summary>
/// Proves a publisher controls a game's domain: either a DNS TXT record <c>openarcade-verify=&lt;token&gt;</c>
/// or the token in a file at <c>/.well-known/openarcade.txt</c>.
/// </summary>
internal sealed class DomainVerifier(HttpClient http, ILookupClient dns, ILogger<DomainVerifier> log) : IDomainVerifier
{
    public async Task<bool> VerifyAsync(string playUrl, string token, CancellationToken ct = default)
    {
        var uri = new Uri(playUrl);
        return await CheckWellKnownAsync(uri, token, ct) || await CheckDnsAsync(uri.Host, token, ct);
    }

    private async Task<bool> CheckWellKnownAsync(Uri playUri, string token, CancellationToken ct)
    {
        try
        {
            var url = new Uri(playUri, "/.well-known/openarcade.txt");
            var body = await http.GetStringAsync(url, ct);
            return body.Split('\n').Any(line => line.Trim() == token);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            log.LogDebug(ex, "Well-known check failed for {Host}", playUri.Host);
            return false;
        }
    }

    private async Task<bool> CheckDnsAsync(string host, string token, CancellationToken ct)
    {
        try
        {
            var result = await dns.QueryAsync(host, QueryType.TXT, cancellationToken: ct);
            return result.Answers.TxtRecords()
                .SelectMany(r => r.Text)
                .Any(t => t.Trim() == $"openarcade-verify={token}");
        }
        catch (DnsResponseException ex)
        {
            log.LogDebug(ex, "DNS check failed for {Host}", host);
            return false;
        }
    }
}
