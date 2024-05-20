namespace LoreSoft.Blazor.Controls.Utilities;


public struct StyleBuilder
{
    private string _buffer;

    public static StyleBuilder Default(string prop, string value) => new StyleBuilder(prop, value);

    public static StyleBuilder Default(string style) => Empty().AddStyle(style);

    public static StyleBuilder Empty() => new StyleBuilder();

    public StyleBuilder(string prop, string value)
        => _buffer = $"{prop}:{value};";

    public StyleBuilder AddStyle(string style)
        => !string.IsNullOrWhiteSpace(style) ? AddRaw($"{style};") : this;

    private StyleBuilder AddRaw(string style)
    {
        _buffer += style;
        return this;
    }

    public StyleBuilder AddStyle(string prop, string value)
        => AddRaw($"{prop}:{value};");

    public StyleBuilder AddStyle(string prop, string value, bool when)
        => when ? AddStyle(prop, value) : this;

    public StyleBuilder AddStyle(string prop, Func<string> value, bool when)
        => when ? AddStyle(prop, value()) : this;

    public StyleBuilder AddStyle(string prop, string value, Func<bool> when)
        => AddStyle(prop, value, when());

    public StyleBuilder AddStyle(string prop, string value, Func<string, bool> when)
        => AddStyle(prop, value, when(value));

    public StyleBuilder AddStyle(string prop, Func<string> value, Func<bool> when)
        => AddStyle(prop, value(), when());

    public StyleBuilder AddStyle(StyleBuilder builder)
        => AddRaw(builder.ToString());

    public StyleBuilder AddStyle(StyleBuilder builder, bool when)
        => when ? AddRaw(builder.ToString()) : this;

    public StyleBuilder AddStyle(StyleBuilder builder, Func<bool> when)
        => AddStyle(builder, when());

    public StyleBuilder MergeStyle(IReadOnlyDictionary<string, object> attributes)
    {
        if (attributes == null)
            return this;

        if (attributes.TryGetValue("style", out var c))
            return AddRaw(c.ToString());

        return this;
    }

    public override readonly string ToString()
    {
        var value = _buffer?.Trim();
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
