#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LoreSoft.Blazor.Controls.Extensions;

/// <summary>
/// <see cref="StringBuilder"/> extension methods
/// </summary>
public static class StringBuilderExtensions
{
    /// <summary>
    /// Appends a copy of the specified string followed by the default line terminator to the end of the StringBuilder object.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static StringBuilder AppendLine(this StringBuilder sb, [StringSyntax("CompositeFormat")] string format, params object[] args)
    {
        sb.AppendFormat(format, args);
        sb.AppendLine();
        return sb;
    }

    /// <summary>
    /// Appends a copy of the specified string if <paramref name="condition"/> is met.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="text">The string to append.</param>
    /// <param name="condition">The condition delegate to evaluate. If condition is null, String.IsNullOrWhiteSpace method will be used.</param>
    public static StringBuilder AppendIf(this StringBuilder sb, string? text, Func<string?, bool>? condition = null)
    {
        var c = condition ?? (s => !string.IsNullOrWhiteSpace(s));

        if (c(text))
            sb.Append(text);

        return sb;
    }

    /// <summary>
    /// Appends a copy of the specified string if <paramref name="condition"/> is met.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="text">The string to append.</param>
    /// <param name="condition">The condition.</param>
    public static StringBuilder AppendIf(this StringBuilder sb, string text, bool condition)
    {
        if (condition)
            sb.Append(text);

        return sb;
    }

    /// <summary>
    /// Appends a copy of the specified string followed by the default line terminator if <paramref name="condition"/> is met.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="text">The string to append.</param>
    /// <param name="condition">The condition delegate to evaluate. If condition is null, String.IsNullOrWhiteSpace method will be used.</param>
    public static StringBuilder AppendLineIf(this StringBuilder sb, string text, Func<string, bool>? condition = null)
    {
        var c = condition ?? (s => !string.IsNullOrWhiteSpace(s));

        if (c(text))
            sb.AppendLine(text);

        return sb;
    }

    /// <summary>
    /// Appends a copy of the specified string followed by the default line terminator if <paramref name="condition"/> is met.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="text">The string to append.</param>
    /// <param name="condition">The condition delegate to evaluate. If condition is null, String.IsNullOrWhiteSpace method will be used.</param>
    public static StringBuilder AppendLineIf(this StringBuilder sb, string text, bool condition)
    {
        if (condition)
            sb.AppendLine(text);

        return sb;
    }
}
