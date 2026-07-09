namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// Provides options for building dynamic LINQ expressions.
/// </summary>
public class LinqExpressionOptions
{
    /// <summary>
    /// Gets the default LINQ expression options.
    /// </summary>
    public static LinqExpressionOptions Default => new();

    /// <summary>
    /// Gets LINQ expression options with no optional provider-specific features enabled.
    /// </summary>
    public static LinqExpressionOptions Empty => new() { StringComparison = null };

    /// <summary>
    /// Gets or sets the string comparison to use for string filter methods.
    /// Set to <c>null</c> to emit provider-compatible string methods without a comparison argument.
    /// </summary>
    public StringComparison? StringComparison { get; set; } = System.StringComparison.OrdinalIgnoreCase;
}
