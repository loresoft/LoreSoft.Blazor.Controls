namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// Provides a fluent API for building CSS style strings for components.
/// Supports conditional and dynamic style addition, merging, and formatting.
/// </summary>
public class StyleBuilder
{
    private string _buffer = string.Empty;

    /// <summary>
    /// Creates a new <see cref="StyleBuilder"/> with a single property and value.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="value">The CSS property value.</param>
    /// <returns>A new <see cref="StyleBuilder"/> instance.</returns>
    public static StyleBuilder Default(string prop, string value)
        => new StyleBuilder().AddStyle(prop, value);

    /// <summary>
    /// Creates a new <see cref="StyleBuilder"/> from a raw style string.
    /// </summary>
    /// <param name="style">The raw CSS style string.</param>
    /// <returns>A new <see cref="StyleBuilder"/> instance.</returns>
    public static StyleBuilder Default(string style)
        => Empty().AddStyle(style);

    /// <summary>
    /// Creates an empty <see cref="StyleBuilder"/>.
    /// </summary>
    /// <returns>A new <see cref="StyleBuilder"/> instance with no styles.</returns>
    public static StyleBuilder Empty() => new();

    /// <summary>
    /// Adds a raw style string to the builder if not null or whitespace.
    /// </summary>
    /// <param name="style">The raw CSS style string to add.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string? style)
        => !string.IsNullOrWhiteSpace(style) ? AddRaw($"{style};") : this;

    /// <summary>
    /// Adds a raw style string to the builder.
    /// </summary>
    /// <param name="style">The raw CSS style string to add.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    private StyleBuilder AddRaw(string? style)
    {
        if (!string.IsNullOrWhiteSpace(style))
            _buffer += style;

        return this;
    }

    /// <summary>
    /// Adds a CSS property and value to the builder.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="value">The CSS property value.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string prop, string? value)
        => AddRaw($"{prop}:{value};");

    /// <summary>
    /// Adds a CSS property and value to the builder if the specified condition is true.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="value">The CSS property value.</param>
    /// <param name="when">If true, the style is added; otherwise, ignored.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string prop, string? value, bool when)
        => when ? AddStyle(prop, value) : this;

    /// <summary>
    /// Adds a CSS property and value from a delegate if the specified condition is true.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="value">A delegate that returns the CSS property value.</param>
    /// <param name="when">If true, the style is added; otherwise, ignored.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string prop, Func<string> value, bool when)
        => when ? AddStyle(prop, value()) : this;

    /// <summary>
    /// Adds a CSS property and value to the builder if the specified condition delegate returns true.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="value">The CSS property value.</param>
    /// <param name="when">A delegate that returns true to add the style.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string prop, string? value, Func<bool> when)
        => AddStyle(prop, value, when());

    /// <summary>
    /// Adds a CSS property and value to the builder if the specified condition delegate returns true for the value.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="value">The CSS property value.</param>
    /// <param name="when">A delegate that evaluates the value and returns true to add the style.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string prop, string? value, Func<string?, bool> when)
        => AddStyle(prop, value, when(value));

    /// <summary>
    /// Adds a CSS property and value from a delegate if the specified condition delegate returns true.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="value">A delegate that returns the CSS property value.</param>
    /// <param name="when">A delegate that returns true to add the style.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string prop, Func<string> value, Func<bool> when)
        => AddStyle(prop, value(), when());

    /// <summary>
    /// Adds styles from another <see cref="StyleBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="StyleBuilder"/> to add.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(StyleBuilder builder)
        => AddRaw(builder.ToString());

    /// <summary>
    /// Adds styles from another <see cref="StyleBuilder"/> if the specified condition is true.
    /// </summary>
    /// <param name="builder">The <see cref="StyleBuilder"/> to add.</param>
    /// <param name="when">If true, the styles are added; otherwise, ignored.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(StyleBuilder builder, bool when)
        => when ? AddRaw(builder.ToString()) : this;

    /// <summary>
    /// Adds styles from another <see cref="StyleBuilder"/> if the specified condition delegate returns true.
    /// </summary>
    /// <param name="builder">The <see cref="StyleBuilder"/> to add.</param>
    /// <param name="when">A delegate that returns true to add the styles.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(StyleBuilder builder, Func<bool> when)
        => AddStyle(builder, when());

    /// <summary>
    /// Merges a "style" attribute from a dictionary of attributes, removing it from the dictionary.
    /// </summary>
    /// <param name="attributes">A dictionary of attributes that may contain a "style" entry.</param>
    /// <param name="remove">Whether to remove the "style" entry from the dictionary.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder MergeStyle(IDictionary<string, object>? attributes, bool remove = true)
    {
        if (attributes == null)
            return this;

        if (!attributes.TryGetValue("style", out var c))
            return this;

        // remove style to prevent duplication
        if (remove)
            attributes.Remove("style");

        return AddRaw(c?.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Returns the built CSS style string.
    /// </summary>
    /// <returns>The CSS style string.</returns>
    public override string ToString()
        => _buffer?.Trim() ?? string.Empty;
}
