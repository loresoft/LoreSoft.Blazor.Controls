// Ignore Spelling: Css

using System.Runtime.CompilerServices;
using System.Text;

using LoreSoft.Blazor.Controls.Extensions;

namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// Provides a fluent API for building CSS class strings for components.
/// Supports conditional and dynamic class addition, merging, and formatting.
/// </summary>
public class CssBuilder()
{
    /// <summary>
    /// Gets a shared singleton instance of <see cref="ObjectPool{T}"/> for <see cref="CssBuilder"/>.
    /// </summary>
    /// <value>A singleton object pool instance for <see cref="CssBuilder"/>.</value>
    /// <remarks>
    /// This provides a convenient way to reuse <see cref="CssBuilder"/> instances and reduce allocations.
    /// The pool is configured with a reset action that clears the builder's state.
    /// </remarks>
    /// <example>
    /// <code>
    /// using var pooled = CssBuilder.Pool.GetPooled();
    /// pooled.Instance
    ///     .AddClass("foo")
    ///     .AddClass("bar", condition);
    /// var classes = pooled.Instance.ToString();
    /// </code>
    /// </example>
    public static ObjectPool<CssBuilder> Pool { get; }
        = new(static () => new CssBuilder(), static builder => builder.Clear());

    /// <summary>
    /// Creates a new <see cref="CssBuilder"/> with an optional initial value.
    /// </summary>
    /// <param name="value">The initial CSS class string.</param>
    /// <returns>A new <see cref="CssBuilder"/> instance.</returns>
    public static CssBuilder Default(string? value = null)
        => new CssBuilder().AddClass(value);

    /// <summary>
    /// Creates an empty <see cref="CssBuilder"/>.
    /// </summary>
    /// <returns>A new <see cref="CssBuilder"/> instance with no classes.</returns>
    public static CssBuilder Empty() => new();


    private readonly StringBuilder _buffer = new(256);

    /// <summary>
    /// Adds a CSS class to the builder if the value is not null or empty.
    /// </summary>
    /// <param name="value">The CSS class to add.</param>
    /// <returns>The current <see cref="CssBuilder"/> instance.</returns>
    public CssBuilder AddClass(string? value)
    {
        if (value.HasValue())
        {
            if (_buffer.Length > 0)
                _buffer.Append(' ');

            _buffer.Append(value);
        }

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
    /// <param name="remove">Whether to remove the "class" entry from the dictionary.</param>
    /// <returns>The current <see cref="CssBuilder"/> instance.</returns>
    [OverloadResolutionPriority(0)]
    public CssBuilder MergeClass(IReadOnlyDictionary<string, object>? attributes, bool remove = true)
    {
        if (attributes == null)
            return this;

        if (!attributes.TryGetValue("class", out var value))
            return this;

        // remove style to prevent duplication
        if (remove && attributes is IDictionary<string, object> dictionary)
            dictionary.Remove("class");

        return AddClass(value?.ToString());
    }

    /// <summary>
    /// Merges a "class" attribute from a dictionary of attributes, removing it from the dictionary.
    /// </summary>
    /// <param name="attributes">A dictionary of attributes that may contain a "class" entry.</param>
    /// <param name="remove">Whether to remove the "class" entry from the dictionary.</param>
    /// <returns>The current <see cref="CssBuilder"/> instance.</returns>
    [OverloadResolutionPriority(9)]
    public CssBuilder MergeClass(IDictionary<string, object>? attributes, bool remove = true)
    {
        if (attributes == null)
            return this;

        if (!attributes.TryGetValue("class", out var value))
            return this;

        // remove style to prevent duplication
        if (remove)
            attributes.Remove("class");

        return AddClass(value?.ToString());
    }

    /// <summary>
    /// Clears the accumulated CSS classes.
    /// </summary>
    public void Clear()
    {
        _buffer.Clear();

        // Reset capacity if it has grown too large to prevent memory bloat
        if (_buffer.Capacity > 1024)
            _buffer.Capacity = 256;
    }

    /// <summary>
    /// Returns the built CSS class string.
    /// </summary>
    /// <returns>The CSS class string.</returns>
    public override string? ToString()
    {
        if (_buffer.Length == 0)
            return null;

        var result = _buffer.ToString().Trim();
        return string.IsNullOrEmpty(result) ? null : result;
    }
}
