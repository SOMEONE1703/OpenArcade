namespace OpenArcade.Domain.Abstractions;

/// <summary>Screens user-written text, such as comments, for blocked words.</summary>
public interface IContentFilter
{
    bool IsClean(string text);
}
