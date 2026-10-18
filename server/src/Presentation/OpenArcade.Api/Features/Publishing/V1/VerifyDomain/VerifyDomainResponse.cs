namespace OpenArcade.Api.Features.Publishing.V1.VerifyDomain;

/// <param name="Instructions">How to publish the token, shown while the domain isn't verified.</param>
public sealed record VerifyDomainResponse(bool Verified, string Instructions);
