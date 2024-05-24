#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace LoreSoft.Blazor.Controls.Extensions;

/// <summary>
/// <see cref="String"/> extension methods
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// Truncates the specified text.
    /// </summary>
    /// <param name="text">The text to truncate.</param>
    /// <param name="keep">The number of characters to keep.</param>
    /// <param name="ellipsis">The ellipsis string to use when truncating. (Default ...)</param>
    /// <returns>
    /// A truncate string.
    /// </returns>
    [return: NotNullIfNotNull(nameof(text))]
    public static string? Truncate(this string? text, int keep, string ellipsis = "...")
    {
        if (string.IsNullOrEmpty(text))
            return text;

        if (string.IsNullOrEmpty(ellipsis))
            ellipsis = string.Empty;

        if (text.Length <= keep)
            return text;

        if (text.Length <= keep + ellipsis.Length || keep < ellipsis.Length)
            return text.Substring(0, keep);

        return string.Concat(text.Substring(0, keep - ellipsis.Length), ellipsis);
    }

    /// <summary>
    /// Indicates whether the specified String object is null or an empty string
    /// </summary>
    /// <param name="item">A String reference</param>
    /// <returns>
    ///     <c>true</c> if is null or empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? item)
    {
        return string.IsNullOrEmpty(item);
    }

    /// <summary>
    /// Indicates whether a specified string is null, empty, or consists only of white-space characters
    /// </summary>
    /// <param name="item">A String reference</param>
    /// <returns>
    ///      <c>true</c> if is null or empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? item)
    {
        if (item == null)
            return true;

        for (int i = 0; i < item.Length; i++)
            if (!char.IsWhiteSpace(item[i]))
                return false;

        return true;
    }

    /// <summary>
    /// Determines whether the specified string is not <see cref="IsNullOrEmpty"/>.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    ///   <c>true</c> if the specified <paramref name="value"/> is not <see cref="IsNullOrEmpty"/>; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasValue([NotNullWhen(true)] this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    [return: NotNullIfNotNull(nameof(value))]
    public static string? ToTitle(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var words = WordRegex().Matches(value);

        var spacedName = new StringBuilder();
        foreach (Match word in words)
        {
            if (spacedName.Length > 0)
                spacedName.Append(' ');

            spacedName.Append(word.Value);
        }

        return spacedName.ToString();
    }

    [GeneratedRegex(@"([A-Z][a-z]*)|([0-9]+)")]
    private static partial Regex WordRegex();
}
