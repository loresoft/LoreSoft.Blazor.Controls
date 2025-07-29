#nullable enable

namespace LoreSoft.Blazor.Controls.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="Type"/> objects, including nullability and default value checks.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Gets the underlying type of a <see cref="Nullable{T}"/> type, or returns the original type if it is not nullable.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// The underlying type if <paramref name="type"/> is a <see cref="Nullable{T}"/>; otherwise, the original <paramref name="type"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
    public static Type GetUnderlyingType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        if (isNullable)
            return Nullable.GetUnderlyingType(type) ?? type;

        return type;
    }

    /// <summary>
    /// Determines whether the specified <see cref="Type"/> can be assigned a <c>null</c> value.
    /// </summary>
    /// <param name="type">The type to check for nullability.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="type"/> is a reference type or a <see cref="Nullable{T}"/> value type; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
    public static bool IsNullable(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (!type.IsValueType)
            return true;

        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// Gets the default value for the specified <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The type to get a default value for.</param>
    /// <returns>
    /// The default value for <paramref name="type"/>; <c>null</c> for reference types, or an instance with default values for value types.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
    public static object? Default(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.IsValueType
          ? Activator.CreateInstance(type)
          : null;
    }
}
