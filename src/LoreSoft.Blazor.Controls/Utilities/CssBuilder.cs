// Ignore Spelling: Css

using LoreSoft.Blazor.Controls.Extensions;

namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// Provides a fluent API for building CSS class strings for components.
/// Supports conditional and dynamic class addition, merging, and formatting.
/// </summary>
public struct CssBuilder(string value)
{
    private string _buffer = value ?? string.Empty;

    /// <summary>
    /// Creates a new <see cref="CssBuilder"/> with an optional initial value.
    /// </summary>
    /// <param name="value">The initial CSS class string.</param>
    /// <returns>A new <see cref="CssBuilder"/> instance.</returns>
    public static CssBuilder Default(string? value = null) => new(value ?? string.Empty);

    /// <summary>
    /// Creates an empty <see cref="CssBuilder"/>.
    /// </summary>
    /// <returns>A new <see cref="CssBuilder"/> instance with no classes.</returns>
    public static CssBuilder Empty() => new(string.Empty);

    /// <summary>
    /// Adds a CSS class to the builder if the value is not null or empty.
    /// </summary>
    /// <param name="value">The CSS class to add.</param>
    /// <returns>The current <see cref="CssBuilder"/> instance.</returns>
    public CssBuilder AddClass(string? value)
    {
        if (value.HasValue())
            _buffer += $" {value}";

        return this;
    }

    /// <summary>
    /// Adds a CSS class if the specified condition is true.
    /// </summary>
    /// <param name="value">The CSS class to add.</param>
    /// <param name="when">If true, the class is added; otherwise, ignored.</param>
    /// <returns>The current <see cref="CssBuilder"/> instance.</returns>
    public CssBuilder AddClass(string? value, bool when)
        => when ? AddClass(value) : this;

    /// <summary>
    /// Adds a CSS class if the specified condition delegate returns true.
    /// </summary>
    /// <param name="value">The CSS class to add.</param>
    /// <param name="when">A delegate that returns true to add the class.</param>
    /// <returns>The current <see cref="CssBuilder"/> instance.</returns>
    public CssBuilder AddClass(string? value, Func<bool> when)
        => AddClass(value, when());

    /// <summary>
    /// Adds a CSS class if the specified condition delegate returns true for the value.
    /// </summary>
    /// <param name="value">The CSS class to add.</param>
    /// <param name="when">A delegate that evaluates the value and returns true to add the class.</param>
    /// <returns>The current <see cref="CssBuilder"/> instance.</returns>
    public CssBuilder AddClass(string? value, Func<string?, bool> when)
        => AddClass(value, when(value));

    /// <summary>
    /// Adds a CSS class from a delegate if the specified condition is true.
    /// </summary>
    /// <param name="value">A delegate that returns the CSS class to add.</param>
    /// <param name="when">If true, the class is added; otherwise, ignored.</param>
    /// <returns>The current <see cref="CssBuilder"/> instance.</returns>
    public CssBuilder AddClass(Func<string?> value, bool when)
        => when ? AddClass(value()) : this;

    /// <summary>
    /// Adds a CSS class from a delegate if the specified condition delegate returns true.
    /// </summary>
    /// <param name="value">A delegate that returns the CSS class to add.</param>
    /// <param name="when">A delegate that returns true to add the class.</param>
    /// <returns>The current <see cref="CssBuilder"/> instance.</returns>
    public CssBuilder AddClass(Func<string?> value, Func<bool> when)
        => AddClass(value, when());

    /// <summary>
    /// Adds a CSS class from another <see cref="CssBuilder"/> if the specified condition is true.
    /// </summary>
    /// <param name="builder">The <see cref="CssBuilder"/> to add.</param>
    /// <param name="when">If true, the class is added; otherwise, ignored.</param>
    /// <returns>The current <see cref="CssBuilder"/> instance.</returns>
    public CssBuilder AddClass(CssBuilder builder, bool when)
        => when ? AddClass(builder.ToString()) : this;

    /// <summary>
    /// Adds a CSS class from another <see cref="CssBuilder"/> if the specified condition delegate returns true.
    /// </summary>
    /// <param name="builder">The <see cref="CssBuilder"/> to add.</param>
    /// <param name="when">A delegate that returns true to add the class.</param>
    /// <returns>The current <see cref="CssBuilder"/> instance.</returns>
    public CssBuilder AddClass(CssBuilder builder, Func<bool> when)
        => AddClass(builder, when());

    /// <summary>
    /// Merges a "class" attribute from a dictionary of attributes, removing it from the dictionary.
    /// </summary>
    /// <param name="attributes">A dictionary of attributes that may contain a "class" entry.</param>
    /// <returns>The current <see cref="CssBuilder"/> instance.</returns>
    public CssBuilder MergeClass(IReadOnlyDictionary<string, object>? attributes)
    {
        if (attributes == null)
            return this;

        if (!attributes.TryGetValue("class", out var value))
            return this;

        if (attributes is IDictionary<string, object> dictionary)
            dictionary.Remove("class");

        return AddClass(value?.ToString());
    }

    /// <summary>
    /// Returns the built CSS class string.
    /// </summary>
    /// <returns>The CSS class string.</returns>
    public override readonly string ToString()
        => _buffer.Trim();
}
