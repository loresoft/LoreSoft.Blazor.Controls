using System.Runtime.CompilerServices;
using System.Text;

namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// Provides a fluent API for building CSS style strings for components.
/// Supports conditional and dynamic style addition, merging, and formatting.
/// </summary>
public class StyleBuilder
{
    /// <summary>
    /// Gets a shared singleton instance of <see cref="ObjectPool{T}"/> for <see cref="StyleBuilder"/>.
    /// </summary>
    /// <value>A singleton object pool instance for <see cref="StyleBuilder"/>.</value>
    /// <remarks>
    /// This provides a convenient way to reuse <see cref="StyleBuilder"/> instances and reduce allocations.
    /// The pool is configured with a reset action that clears the builder's state.
    /// </remarks>
    /// <example>
    /// <code>
    /// using var pooled = StyleBuilder.Pool.GetPooled();
    /// pooled.Instance
    ///     .AddStyle("color", "red")
    ///     .AddStyle("padding", "10px", condition);
    /// var styles = pooled.Instance.ToString();
    /// </code>
    /// </example>
    public static ObjectPool<StyleBuilder> Pool { get; }
        = new(static () => new StyleBuilder(), static builder => builder.Clear());

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
    public static StyleBuilder Default(string? style)
        => new StyleBuilder().AddStyle(style);

    /// <summary>
    /// Creates an empty <see cref="StyleBuilder"/>.
    /// </summary>
    /// <returns>A new <see cref="StyleBuilder"/> instance with no styles.</returns>
    public static StyleBuilder Empty() => new();


    private readonly StringBuilder _buffer = new(256);

    /// <summary>
    /// Adds a raw style string to the builder if not null or whitespace.
    /// </summary>
    /// <param name="style">The raw CSS style string to add.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string? style)
        => AddRaw(style);

    /// <summary>
    /// Adds a raw style string to the builder.
    /// </summary>
    /// <param name="style">The raw CSS style string to add.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    private StyleBuilder AddRaw(string? style)
    {
        if (string.IsNullOrWhiteSpace(style))
            return this;

        _buffer.Append(style);

        // Ensure style ends with semicolon
        if (!style.EndsWith(';'))
            _buffer.Append(';');

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
    [OverloadResolutionPriority(0)]
    public StyleBuilder MergeStyle(IReadOnlyDictionary<string, object>? attributes, bool remove = true)
    {
        if (attributes == null)
            return this;

        if (!attributes.TryGetValue("style", out var c))
            return this;

        // remove style to prevent duplication
        if (remove && attributes is IDictionary<string, object> dictionary)
            dictionary.Remove("style");

        return AddRaw(c?.ToString());
    }

    /// <summary>
    /// Merges a "style" attribute from a dictionary of attributes, removing it from the dictionary.
    /// </summary>
    /// <param name="attributes">A dictionary of attributes that may contain a "style" entry.</param>
    /// <param name="remove">Whether to remove the "style" entry from the dictionary.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    [OverloadResolutionPriority(9)]
    public StyleBuilder MergeStyle(IDictionary<string, object>? attributes, bool remove = true)
    {
        if (attributes == null)
            return this;

        if (!attributes.TryGetValue("style", out var c))
            return this;

        // remove style to prevent duplication
        if (remove)
            attributes.Remove("style");

        return AddRaw(c?.ToString());
    }

    /// <summary>
    /// Clears the accumulated CSS styles.
    /// </summary>
    public void Clear()
    {
        _buffer.Clear();

        // Reset capacity if it has grown too large to prevent memory bloat
        if (_buffer.Capacity > 1024)
            _buffer.Capacity = 256;
    }

    /// <summary>
    /// Returns the built CSS style string.
    /// </summary>
    /// <returns>The CSS style string.</returns>
    public override string? ToString()
    {
        if (_buffer.Length == 0)
            return null;

        var result = _buffer.ToString().Trim();
        return string.IsNullOrEmpty(result) ? null : result;
    }
}
