using System;
using System.Collections.Generic;

namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// Provides a fluent API for building a dictionary of HTML attributes for components.
/// Supports conditional and dynamic attribute addition.
/// </summary>
public readonly struct AttributeBuilder
{
    private readonly Dictionary<string, object> _attributes;

    /// <summary>
    /// Creates a new <see cref="AttributeBuilder"/> with a single property and value.
    /// </summary>
    /// <param name="prop">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    /// <returns>A new <see cref="AttributeBuilder"/> instance.</returns>
    public static AttributeBuilder Default(string prop, object value)
        => new(prop, value);

    /// <summary>
    /// Creates an empty <see cref="AttributeBuilder"/>.
    /// </summary>
    /// <returns>A new <see cref="AttributeBuilder"/> instance with no attributes.</returns>
    public static AttributeBuilder Empty()
        => new([]);

    /// <summary>
    /// Initializes a new <see cref="AttributeBuilder"/> with the specified attributes.
    /// </summary>
    /// <param name="attributes">A dictionary of attribute name/value pairs.</param>
    public AttributeBuilder(Dictionary<string, object> attributes)
    {
        _attributes = attributes;
    }

    /// <summary>
    /// Initializes a new <see cref="AttributeBuilder"/> with a single property and value.
    /// </summary>
    /// <param name="prop">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    public AttributeBuilder(string prop, object value)
    {
        _attributes = new Dictionary<string, object> { { prop, value } };
    }

    /// <summary>
    /// Adds or updates an attribute in the builder.
    /// </summary>
    /// <param name="prop">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    /// <returns>The current <see cref="AttributeBuilder"/> instance.</returns>
    public AttributeBuilder AddAttribute(string prop, object value)
    {
        _attributes[prop] = value;
        return this;
    }

    /// <summary>
    /// Adds or updates an attribute if the specified condition is true.
    /// </summary>
    /// <param name="prop">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    /// <param name="when">If true, the attribute is added; otherwise, it is ignored.</param>
    /// <returns>The current <see cref="AttributeBuilder"/> instance.</returns>
    public AttributeBuilder AddAttribute(string prop, object value, bool when)
        => when ? AddAttribute(prop, value) : this;

    /// <summary>
    /// Adds or updates an attribute if the specified condition delegate returns true.
    /// </summary>
    /// <param name="prop">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    /// <param name="when">A delegate that returns true to add the attribute.</param>
    /// <returns>The current <see cref="AttributeBuilder"/> instance.</returns>
    public AttributeBuilder AddAttribute(string prop, object value, Func<bool> when)
        => when() ? AddAttribute(prop, value) : this;

    /// <summary>
    /// Adds or updates an attribute using a value delegate if the specified condition is true.
    /// </summary>
    /// <param name="prop">The attribute name.</param>
    /// <param name="value">A delegate that returns the attribute value.</param>
    /// <param name="when">If true, the attribute is added; otherwise, it is ignored.</param>
    /// <returns>The current <see cref="AttributeBuilder"/> instance.</returns>
    public AttributeBuilder AddAttribute(string prop, Func<object> value, bool when)
        => when ? AddAttribute(prop, value()) : this;

    /// <summary>
    /// Adds or updates an attribute using a value delegate if the specified condition delegate returns true.
    /// </summary>
    /// <param name="prop">The attribute name.</param>
    /// <param name="value">A delegate that returns the attribute value.</param>
    /// <param name="when">A delegate that returns true to add the attribute.</param>
    /// <returns>The current <see cref="AttributeBuilder"/> instance.</returns>
    public AttributeBuilder AddAttribute(string prop, Func<object> value, Func<bool> when)
        => when() ? AddAttribute(prop, value()) : this;

    /// <summary>
    /// Builds and returns the dictionary of attributes.
    /// </summary>
    /// <returns>A dictionary of attribute name/value pairs.</returns>
    public Dictionary<string, object> Build()
        => _attributes;
}
