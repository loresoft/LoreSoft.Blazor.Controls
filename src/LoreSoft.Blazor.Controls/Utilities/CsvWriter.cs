// Ignore Spelling: Css

using System.Buffers;
using System.Text;

namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// Provides functionality to write CSV data to a stream, text writer, or string.
/// </summary>
/// <remarks>
/// This static class supports writing CSV-formatted data asynchronously from a sequence of objects,
/// with proper escaping and optional headers. The data fields are mapped using a selector function,
/// and escaping is applied to match standard CSV conventions (e.g., Excel-compatible).
/// </remarks>
/// <example>
/// The following example demonstrates how to use <c>WriteAsync</c> to write a list of people to CSV:
/// <code>
/// public record Person(string Name, int Age, string Email);
///
/// string[] headers = [nameof(Person.Name), nameof(Person.Age), nameof(Person.Email)];
///
/// List&lt;Person&gt; people =
/// [
///     new("Alice", 30, "alice@example.com"),
///     new("Bob", 25, "bob@example.com"),
///     new("Charlie", 35, "charlie@example.com")
/// ];
///
/// var csvContent = await CsvWriter.WriteAsync(
///     headers: headers,
///     rows: people,
///     selector: p => [p.Name, p.Age.ToString(), p.Email]);
///
/// </code>
/// </example>
public static class CsvWriter
{
    private static readonly SearchValues<char> SpecialChars = SearchValues.Create(",\"\n\r");

    /// <summary>
    /// Asynchronously writes CSV-formatted data to a <see cref="Stream"/>.
    /// </summary>
    /// <typeparam name="T">The type of the data rows.</typeparam>
    /// <param name="stream">The output <see cref="Stream"/> to write the CSV content to.</param>
    /// <param name="headers">Optional headers to write as the first row.</param>
    /// <param name="rows">The data rows to write.</param>
    /// <param name="selector">A function that maps each row to a collection of string fields.</param>
    /// <param name="encoding">The text encoding to use. Defaults to UTF-8 if null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task WriteAsync<T>(
        Stream stream,
        IEnumerable<string>? headers,
        IEnumerable<T> rows,
        Func<T, IEnumerable<string?>> selector,
        Encoding? encoding = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        encoding ??= Encoding.UTF8;

        var writer = new StreamWriter(stream, encoding, bufferSize: 4096, leaveOpen: true);
        await using (writer.ConfigureAwait(false))
        {
            await WriteAsync(writer, headers, rows, selector, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously writes CSV-formatted data to a <see cref="string"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the data rows.</typeparam>
    /// <param name="headers">Optional headers to write as the first row.</param>
    /// <param name="rows">The data rows to write.</param>
    /// <param name="selector">A function that maps each row to a collection of string fields.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> whose result is the CSV-formatted string..</returns>
    public static async Task<string> WriteAsync<T>(
        IEnumerable<string>? headers,
        IEnumerable<T> rows,
        Func<T, IEnumerable<string?>> selector,
        CancellationToken cancellationToken = default)
    {
        var writer = new StringWriter();

        await WriteAsync(writer, headers, rows, selector, cancellationToken)
            .ConfigureAwait(false);

        await writer.FlushAsync(cancellationToken)
            .ConfigureAwait(false);

        return writer.ToString();
    }

    /// <summary>
    /// Asynchronously writes CSV-formatted data to a <see cref="TextWriter"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the data rows.</typeparam>
    /// <param name="writer">The <see cref="TextWriter"/> to which the CSV content will be written.</param>
    /// <param name="headers">Optional headers to write as the first row.</param>
    /// <param name="rows">The data rows to write.</param>
    /// <param name="selector">A function that maps each row to a collection of string fields.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task WriteAsync<T>(
        TextWriter writer,
        IEnumerable<string>? headers,
        IEnumerable<T> rows,
        Func<T, IEnumerable<string?>> selector,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(selector);

        if (headers != null)
        {
            await WriteRowAsync(writer, headers)
                .ConfigureAwait(false);
        }

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await WriteRowAsync(writer, selector(row))
                .ConfigureAwait(false);
        }
    }

    private static async Task WriteRowAsync(
        TextWriter writer,
        IEnumerable<string?> fields)
    {
        bool first = true;

        foreach (var field in fields)
        {
            if (!first)
                await writer.WriteAsync(',').ConfigureAwait(false);

            await WriteFieldAsync(writer, field).ConfigureAwait(false);
            first = false;
        }

        await writer.WriteLineAsync().ConfigureAwait(false);
    }

    private static async Task WriteFieldAsync(
        TextWriter writer,
        string? value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        var span = value.AsSpan();
        var needsQuotes = span.ContainsAny(SpecialChars);

        if (!needsQuotes)
        {
            // Fast path: no special chars, write directly
            await writer.WriteAsync(value).ConfigureAwait(false);
            return;
        }

        // write with quotes and escape any quotes within the value
        await writer.WriteAsync('"').ConfigureAwait(false);
        for (var i = 0; i < value.Length; i++)
        {
            var ch = value[i];

            if (ch == '"')
                await writer.WriteAsync("\"\"").ConfigureAwait(false);
            else
                await writer.WriteAsync(ch).ConfigureAwait(false);
        }
        await writer.WriteAsync('"').ConfigureAwait(false);
    }
}
