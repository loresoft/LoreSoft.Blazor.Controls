using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LoreSoft.Blazor.Controls.Utilities;

namespace LoreSoft.Blazor.Controls.Extensions;

/// <summary>
/// Provides extension methods and pooling support for common .NET types.
/// </summary>
public static class ObjectPoolExtensions
{
    extension(StringBuilder)
    {
        /// <summary>
        /// Provides access to a shared <see cref="ObjectPool{T}"/> for <see cref="StringBuilder"/> instances.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The pool creates <see cref="StringBuilder"/> instances with an initial capacity of 256 characters.
        /// When returned to the pool, instances are cleared and their capacity is trimmed to 256 characters if it exceeds 1024 characters.
        /// </para>
        /// <para>
        /// This pool is lazily initialized and thread-safe for concurrent access.
        /// </para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the pool to build a string efficiently:
        /// <code>
        /// string result = StringBuilder.Pool.Use(sb =>
        /// {
        ///     sb.Append("Hello, ");
        ///     sb.Append("World!");
        ///     return sb.ToString();
        /// });
        /// </code>
        /// </example>
        public static ObjectPool<StringBuilder> Pool => _stringBuilderPool.Value;
    }

    private static readonly Lazy<ObjectPool<StringBuilder>> _stringBuilderPool = new(()
        => new(objectFactory: static () => new(256), resetAction: ResetAction));

    private static void ResetAction(StringBuilder sb)
    {
        sb.Clear();

        // trim excess capacity if it has grown too large
        if (sb.Capacity > 1024)
            sb.Capacity = 256;
    }
}
