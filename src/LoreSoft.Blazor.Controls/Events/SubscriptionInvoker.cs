namespace LoreSoft.Blazor.Controls.Events;

/// <summary>
/// Strongly-typed dispatcher signature used to invoke a cached event handler
/// without boxing the returned <see cref="ValueTask"/>.
/// </summary>
/// <param name="target">The handler's instance target, or <see langword="null"/> for static handlers.</param>
/// <param name="parameter">The event payload, or <see langword="null"/> for parameterless handlers.</param>
/// <param name="cancellationToken">The cancellation token forwarded to the handler when its signature accepts one.</param>
/// <returns>A <see cref="ValueTask"/> representing the (possibly synchronous) handler completion.</returns>
internal delegate ValueTask SubscriptionInvoker(object? target, object? parameter, CancellationToken cancellationToken);
