using System;
using System.Collections.Generic;

namespace LoreSoft.Blazor.Controls.Utilities;

public readonly struct AttributeBuilder
{
    private readonly Dictionary<string, object> _attributes;

    public static AttributeBuilder Default(string prop, object value)
        => new(prop, value);

    public static AttributeBuilder Empty()
        => new([]);

    public AttributeBuilder(Dictionary<string, object> attributes)
    {
        _attributes = attributes;
    }

    public AttributeBuilder(string prop, object value)
    {
        _attributes = new Dictionary<string, object> { { prop, value } };
    }

    public AttributeBuilder AddAttribute(string prop, object value)
    {
        _attributes[prop] = value;
        return this;
    }

    public AttributeBuilder AddAttribute(string prop, object value, bool when)
        => when ? AddAttribute(prop, value) : this;

    public AttributeBuilder AddAttribute(string prop, object value, Func<bool> when)
        => when() ? AddAttribute(prop, value) : this;

    public AttributeBuilder AddAttribute(string prop, Func<object> value, bool when)
        => when ? AddAttribute(prop, value()) : this;

    public AttributeBuilder AddAttribute(string prop, Func<object> value, Func<bool> when)
        => when() ? AddAttribute(prop, value()) : this;

    public Dictionary<string, object> Build()
        => _attributes;
}
