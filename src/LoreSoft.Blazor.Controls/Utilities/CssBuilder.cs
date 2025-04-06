// Ignore Spelling: Css

using LoreSoft.Blazor.Controls.Extensions;

namespace LoreSoft.Blazor.Controls.Utilities;

public struct CssBuilder(string? value)
{
    private string _buffer = value ?? string.Empty;

    public static CssBuilder Default(string? value = null) => new(value);

    public static CssBuilder Empty() => new();

    public CssBuilder AddClass(string? value)
    {
        if (value.HasValue())
            _buffer += $" {value}";

        return this;
    }

    public CssBuilder AddClass(string? value, bool when)
        => when ? AddClass(value) : this;

    public CssBuilder AddClass(string? value, Func<bool> when)
        => AddClass(value, when());

    public CssBuilder AddClass(string? value, Func<string?, bool> when)
        => AddClass(value, when(value));

    public CssBuilder AddClass(Func<string?> value, bool when)
        => when ? AddClass(value()) : this;

    public CssBuilder AddClass(Func<string?> value, Func<bool> when)
        => AddClass(value, when());

    public CssBuilder AddClass(CssBuilder builder, bool when)
        => when ? AddClass(builder.ToString()) : this;

    public CssBuilder AddClass(CssBuilder builder, Func<bool> when)
        => AddClass(builder, when());

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

    public override readonly string ToString()
        => _buffer.Trim();
}
