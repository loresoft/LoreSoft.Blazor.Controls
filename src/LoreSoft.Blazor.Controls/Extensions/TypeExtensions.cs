#nullable enable

namespace LoreSoft.Blazor.Controls.Extensions;

/// <summary>
/// <see cref="T:Type"/> extension methods.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Gets the underlying type dealing with <see cref="T:Nullable`1"/>.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>Returns a type dealing with <see cref="T:Nullable`1"/>.</returns>
    public static Type GetUnderlyingType(this Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        if (isNullable)
            return Nullable.GetUnderlyingType(type) ?? type;

        return type;
    }

    /// <summary>
    /// Determines whether the specified <paramref name="type"/> can be null.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>
    ///   <c>true</c> if the specified <paramref name="type"/> can be null; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullable(this Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (!type.IsValueType)
            return true;

        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// Gets the default value the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to get a default value for.</param>
    /// <returns>A default value the specified <paramref name="type"/>.</returns>
    public static object? Default(this Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        return type.IsValueType
          ? Activator.CreateInstance(type)
          : null;
    }
}
