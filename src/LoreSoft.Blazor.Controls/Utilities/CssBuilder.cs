using System;
using System.Collections.Generic;

namespace LoreSoft.Blazor.Controls.Utilities
{
    public struct CssBuilder
    {
        private string _buffer;

        public static CssBuilder Default(string value) => new(value);

        public static CssBuilder Empty() => new();

        public CssBuilder(string value) => _buffer = value;

        public CssBuilder AddClass(string value)
        {
            _buffer += $" {value}";
            return this;
        }

        public CssBuilder AddClass(string value, bool when) => when ? this.AddClass(value) : this;

        public CssBuilder AddClass(string value, Func<bool> when) => this.AddClass(value, when());

        public CssBuilder AddClass(Func<string> value, bool when) => when ? this.AddClass(value()) : this;

        public CssBuilder AddClass(Func<string> value, Func<bool> when) => this.AddClass(value, when());

        public CssBuilder AddClass(CssBuilder builder, bool when) => when ? this.AddClass(builder.ToString()) : this;

        public CssBuilder AddClass(CssBuilder builder, Func<bool> when) => this.AddClass(builder, when());

        public CssBuilder AddClass(IReadOnlyDictionary<string, object> attributes) =>
            attributes == null ? this :
            attributes.TryGetValue("class", out var c) ? AddClass(c.ToString()) : this;

        public override string ToString()
        {
            var value = _buffer?.Trim();
            return string.IsNullOrEmpty(value) ? null : value;
        }

    }
}
