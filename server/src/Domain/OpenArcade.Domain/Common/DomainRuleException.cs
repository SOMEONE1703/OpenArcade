namespace OpenArcade.Domain.Common;

/// <summary>Thrown when an action breaks a business rule. The API turns it into a 400 response.</summary>
public sealed class DomainRuleException(string message) : Exception(message);
