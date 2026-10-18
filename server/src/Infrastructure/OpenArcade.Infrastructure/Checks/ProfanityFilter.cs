using System.Text.RegularExpressions;
using OpenArcade.Domain.Abstractions;

namespace OpenArcade.Infrastructure.Checks;

/// <summary>
/// Minimal word-list filter run when a comment is posted. Swap for a maintained list or service later;
/// extend the list via the "Moderation:BlockedWords" setting.
/// </summary>
internal sealed class ProfanityFilter : IContentFilter
{
    private readonly Regex _pattern;

    public ProfanityFilter(IConfiguration config)
    {
        var words = config.GetSection("Moderation:BlockedWords").Get<string[]>() ?? [];
        _pattern = words.Length == 0
            ? new Regex("(?!)") // matches nothing
            : new Regex($@"\b({string.Join("|", words.Select(Regex.Escape))})\b",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    public bool IsClean(string text) => !_pattern.IsMatch(text);
}
