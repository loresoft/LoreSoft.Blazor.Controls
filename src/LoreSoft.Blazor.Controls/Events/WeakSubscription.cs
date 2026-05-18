using System.Collections.Concurrent;
using System.Reflection;

namespace LoreSoft.Blazor.Controls.Events;

/// <summary>
/// Represents a single weak-event subscription: a weakly-held target plus a
/// cached <see cref="MethodDescriptor"/> describing how to invoke the handler.
/// </summary>
/// <remarks>
/// <para>
/// The target is held via the inherited <see cref="WeakReference"/> so subscribers can be garbage
/// collected without first unsubscribing. By inheriting from <see cref="WeakReference"/> rather than
/// composing one as a field, every subscribe avoids the second heap allocation that a separate
/// <see cref="WeakReference"/> instance would incur.
/// </para>
/// <para>
/// Static handlers have no target to weakly hold; they reuse a process-wide sentinel so the base
/// <see cref="WeakReference"/> can still be initialized with a non-null reference. The
/// <see cref="MethodDescriptor.IsStatic"/> flag short-circuits target lookups for those subscriptions,
/// so the sentinel is never observed by callers.
/// </para>
/// </remarks>
internal sealed class WeakSubscription : WeakReference
{
    /// <summary>
    /// Process-wide cache of <see cref="MethodDescriptor"/> instances keyed by <see cref="MethodInfo"/>.
    /// Ensures each unique handler signature is reflected over and compiled only once.
    /// </summary>
    private static readonly ConcurrentDictionary<MethodInfo, MethodDescriptor> MethodCache = new();

    /// <summary>
    /// Shared sentinel passed to the base <see cref="WeakReference"/> when the handler is static.
    /// The sentinel is a single long-lived instance; <see cref="MethodDescriptor.IsStatic"/> guards
    /// every code path that would otherwise dereference it, so its identity does not leak.
    /// </summary>
    private static readonly object StaticSentinel = new();

    private readonly MethodDescriptor _descriptor;

    /// <summary>
    /// Initializes a new <see cref="WeakSubscription"/> for the supplied <paramref name="handler"/>.
    /// </summary>
    /// <param name="handler">The delegate to subscribe. May target an instance method or a static method.</param>
    public WeakSubscription(Delegate handler)
        : base(handler.Target ?? StaticSentinel)
    {
        _descriptor = MethodCache.GetOrAdd(handler.Method, static method => new MethodDescriptor(method));
    }

    /// <summary>
    /// Gets a value indicating whether this subscription's target is still alive (or the handler is static).
    /// </summary>
    public override bool IsAlive => _descriptor.IsStatic || Target is not null;

    /// <summary>
    /// Attempts to invoke the subscription's handler.
    /// </summary>
    /// <param name="parameter">The event payload to forward to the handler.</param>
    /// <param name="cancellationToken">The cancellation token to forward to the handler.</param>
    /// <param name="task">When this method returns <see langword="true"/>, contains the <see cref="ValueTask"/>
    /// returned by the handler; otherwise <see langword="default"/>.</param>
    /// <returns><see langword="true"/> when the handler was invoked; <see langword="false"/> when the weak
    /// target has been garbage collected.</returns>
    public bool TryInvoke(object? parameter, CancellationToken cancellationToken, out ValueTask task)
    {
        object? target;

        if (_descriptor.IsStatic)
        {
            target = null;
        }
        else
        {
            target = Target;
            if (target is null)
            {
                task = default;
                return false;
            }
        }

        task = _descriptor.Invoker(target, parameter, cancellationToken);
        return true;
    }

    /// <summary>
    /// Determines whether this subscription was created from the same delegate as <paramref name="handler"/>
    /// (same target instance and same <see cref="MethodInfo"/>).
    /// </summary>
    /// <param name="handler">The delegate to compare against.</param>
    /// <returns><see langword="true"/> when both the target and method match; otherwise <see langword="false"/>.</returns>
    public bool IsDelegateMatch(Delegate handler)
    {
        var target = _descriptor.IsStatic ? null : Target;

        return (_descriptor.IsStatic || target is not null)
            && target == handler.Target
            && _descriptor.Method.Equals(handler.Method);
    }

    /// <summary>
    /// Determines whether this subscription's live target is the same instance as <paramref name="target"/>.
    /// Used to bulk-unsubscribe all handlers owned by a given subscriber object.
    /// </summary>
    /// <param name="target">The subscriber instance to test.</param>
    /// <returns><see langword="true"/> when the weak target is still alive and equals <paramref name="target"/>;
    /// otherwise <see langword="false"/>. Always <see langword="false"/> for static handlers.</returns>
    public bool IsTargetMatch(object target)
        => !_descriptor.IsStatic && ReferenceEquals(Target, target);
}
