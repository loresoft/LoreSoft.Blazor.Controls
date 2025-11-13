using System.Reflection;

namespace LoreSoft.Blazor.Controls.Events;

/// <summary>
/// Represents a weak reference to a delegate, allowing the target object to be garbage collected
/// while maintaining the ability to invoke the delegate if the target is still alive.
/// </summary>
/// <remarks>
/// <para>
/// This struct uses weak references to prevent memory leaks from event handlers while supporting
/// efficient invocation through compiled expressions instead of reflection.
/// </para>
/// <para>
/// The delegate can be invoked with optional parameters and cancellation tokens, depending on
/// the signature of the original delegate method.
/// </para>
/// </remarks>
public readonly struct WeakDelegate
{
    private readonly WeakReference? _weakTarget;
    private readonly MethodInfo _method;
    private readonly MethodInvoker _methodInvoker;
    private readonly bool _supportParameter;
    private readonly bool _supportCancellation;
    private readonly bool _isStatic;
    private readonly int _invocationMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeakDelegate"/> struct with the specified delegate.
    /// </summary>
    /// <param name="handler">The delegate to wrap in a weak reference.</param>
    /// <remarks>
    /// <para>
    /// Creates a compiled method invoker for fast invocation without reflection overhead.
    /// Analyzes the method signature to determine supported invocation modes (with/without parameters and cancellation tokens).
    /// </para>
    /// <para>
    /// If the delegate has a target (non-static), a weak reference is created to allow the target to be garbage collected.
    /// </para>
    /// </remarks>
    public WeakDelegate(Delegate handler)
    {
        _method = handler.Method;

        if (handler.Target != null)
            _weakTarget = new WeakReference(handler.Target);

        // Create compiled invoker for fast invocation without reflection overhead
        _methodInvoker = MethodInvoker.Create(_method);

        _isStatic = _method.IsStatic;

        // Analyze method parameters to determine supported invocation modes
        var parameters = _method.GetParameters();
        if (parameters.Length >= 1)
        {
            _supportCancellation = parameters[^1].ParameterType == typeof(CancellationToken);
            _supportParameter = (parameters.Length == 1 && !_supportCancellation)
                || (parameters.Length == 2 && _supportCancellation);
        }

        // Determine invocation mode based on supported parameters
        _invocationMode = (_supportParameter ? 1 : 0) | (_supportCancellation ? 2 : 0);
    }

    /// <summary>
    /// Gets a value indicating whether the target object of the delegate is still alive and has not been garbage collected.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the delegate is static (no target) or if the target object is still alive;
    /// otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// For static delegates, this property always returns <see langword="true"/>.
    /// For instance delegates, this checks whether the weak reference still holds a valid target.
    /// </remarks>
    public bool IsAlive => _isStatic || (_weakTarget?.Target != null);

    /// <summary>
    /// Invokes the delegate asynchronously if the target is still alive.
    /// </summary>
    /// <param name="parameter">An optional parameter to pass to the delegate. Only used if the delegate signature accepts a parameter.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation. Only passed if the delegate signature accepts a <see cref="CancellationToken"/>.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation.
    /// Returns <see cref="ValueTask.CompletedTask"/> if the target has been garbage collected,
    /// if the delegate does not return a task, or if the delegate returns <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The method automatically matches the invocation signature based on the delegate's parameter requirements:
    /// <list type="bullet">
    /// <item><description>No parameters</description></item>
    /// <item><description>One parameter only</description></item>
    /// <item><description>Cancellation token only</description></item>
    /// <item><description>One parameter and cancellation token</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If the target has been garbage collected, the invocation is skipped and <see cref="ValueTask.CompletedTask"/> is returned immediately.
    /// </para>
    /// </remarks>
    public ValueTask InvokeAsync(object? parameter = null, CancellationToken cancellationToken = default)
    {
        // Capture target once to prevent race condition between check and use
        var target = _isStatic ? null : _weakTarget?.Target;

        // Check the captured target, not the IsAlive property to avoid TOCTOU race
        if (!_isStatic && target == null)
            return ValueTask.CompletedTask;

        // Invoke the method based on supported parameters
        var result = _invocationMode switch
        {
            0 => _methodInvoker.Invoke(target),
            1 => _methodInvoker.Invoke(target, parameter),
            2 => _methodInvoker.Invoke(target, cancellationToken),
            3 => _methodInvoker.Invoke(target, parameter, cancellationToken),
            _ => throw new InvalidOperationException()
        };

        // Use pattern matching for cleaner code
        return result switch
        {
            ValueTask valueTask => valueTask,
            Task task => new ValueTask(task),
            _ => ValueTask.CompletedTask
        };
    }

    /// <summary>
    /// Determines whether the specified delegate matches this weak delegate.
    /// </summary>
    /// <param name="handler">The delegate to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both the target and method of the specified delegate match this weak delegate;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method compares both the target object and the method to determine equality.
    /// Two delegates with the same method but different targets are not considered a match.
    /// </remarks>
    public bool IsDelegateMatch(Delegate handler)
        => _weakTarget?.Target == handler.Target && _method.Equals(handler.Method);

    /// <summary>
    /// Determines whether the specified object is the target of this weak delegate.
    /// </summary>
    /// <param name="target">The object to compare against the delegate's target.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is the target of this weak delegate;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method uses reference equality to compare the target objects.
    /// For static delegates (which have no target), this will return <see langword="false"/> unless the comparison is with <see langword="null"/>.
    /// </remarks>
    public bool IsTargetMatch(object target)
        => _weakTarget?.Target == target;
}
