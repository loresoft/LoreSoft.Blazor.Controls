using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace LoreSoft.Blazor.Controls.Events;

/// <summary>
/// Cached invocation metadata for an event handler <see cref="MethodInfo"/>.
/// Holding this as a <see langword="readonly struct"/> avoids one allocation per
/// unique handler signature and stores inline inside both the dictionary cache and
/// the <see cref="WeakSubscription"/> field that owns it.
/// </summary>
internal readonly struct MethodDescriptor
{
    /// <summary>
    /// Cached reference to the <see cref="ValueTask(Task)"/> constructor used by
    /// <see cref="CompileTypedInvoker"/> to wrap <see cref="Task"/>-returning handlers without boxing.
    /// </summary>
    private static readonly ConstructorInfo ValueTaskFromTaskCtor =
        typeof(ValueTask).GetConstructor([typeof(Task)])!;

    /// <summary>
    /// Cached expression that reads <see cref="ValueTask.CompletedTask"/>, reused by
    /// <see cref="CompileTypedInvoker"/> to terminate synchronous (void) handler bodies.
    /// </summary>
    private static readonly MemberExpression CompletedTaskExpression =
        Expression.Property(null, typeof(ValueTask).GetProperty(nameof(ValueTask.CompletedTask))!);

    /// <summary>
    /// The <see cref="MethodInfo"/> this descriptor describes. Used to identify handlers during
    /// subscribe/unsubscribe and as the dictionary key for the descriptor cache.
    /// </summary>
    public readonly MethodInfo Method;

    /// <summary>
    /// The delegate used to invoke <see cref="Method"/> against a weakly-referenced target. Built once
    /// per <see cref="MethodInfo"/> via <see cref="BuildInvoker"/> and reused for every publish.
    /// </summary>
    public readonly SubscriptionInvoker Invoker;

    /// <summary>
    /// <see langword="true"/> when <see cref="Method"/> is a static method. Allows <see cref="WeakSubscription"/>
    /// to skip allocating a <see cref="WeakReference"/> for the target.
    /// </summary>
    public readonly bool IsStatic;

    /// <summary>
    /// The signature shape of <see cref="Method"/>, used by the publish pipeline to decide which
    /// arguments to pass to <see cref="Invoker"/>.
    /// </summary>
    public readonly InvocationMode InvocationMode;

    /// <summary>
    /// Initializes a new <see cref="MethodDescriptor"/> for <paramref name="method"/>, inspecting its
    /// parameters to determine the <see cref="InvocationMode"/> and compiling a typed invoker.
    /// </summary>
    /// <param name="method">The handler method to describe. Must have 0–2 parameters where an optional
    /// trailing <see cref="CancellationToken"/> parameter is supported.</param>
    public MethodDescriptor(MethodInfo method)
    {
        Method = method;
        IsStatic = method.IsStatic;

        var parameters = method.GetParameters();

        var supportCancellation = parameters.Length >= 1
            && parameters[^1].ParameterType == typeof(CancellationToken);

        var supportParameter = (parameters.Length == 1 && !supportCancellation)
            || (parameters.Length == 2 && supportCancellation);

        InvocationMode = (InvocationMode)((supportParameter ? 1 : 0) | (supportCancellation ? 2 : 0));
        Invoker = BuildInvoker(method);
    }

    /// <summary>
    /// Builds a <see cref="SubscriptionInvoker"/> for <paramref name="method"/>, preferring a typed
    /// expression-compiled delegate and falling back to a reflection-based invoker when dynamic code
    /// generation is unavailable (for example on AOT-compiled WebAssembly hosts).
    /// </summary>
    /// <param name="method">The handler method to wrap.</param>
    /// <returns>A delegate that invokes <paramref name="method"/> and yields a <see cref="ValueTask"/>.</returns>
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Expression compilation may require dynamic code; a reflection-based fallback is used when compilation throws.")]
    private static SubscriptionInvoker BuildInvoker(MethodInfo method)
    {
        try
        {
            return CompileTypedInvoker(method);
        }
        catch
        {
            return CreateFallbackInvoker(method);
        }
    }

    /// <summary>
    /// Builds a strongly-typed <see cref="SubscriptionInvoker"/> by composing a LINQ expression tree
    /// that directly invokes <paramref name="method"/>, avoiding the per-call boxing and parameter-array
    /// allocations that <see cref="MethodBase.Invoke(object, object[])"/> incurs.
    /// </summary>
    /// <param name="method">The handler method to compile an invoker for.</param>
    /// <returns>A compiled delegate that dispatches directly to <paramref name="method"/>.</returns>
    /// <remarks>
    /// May throw on AOT/no-dynamic-code hosts; <see cref="BuildInvoker"/> handles that by falling back
    /// to <see cref="CreateFallbackInvoker"/>.
    /// </remarks>
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Expression.Compile may require dynamic code in some hosts. Callers handle exceptions and fall back to reflection.")]
    private static SubscriptionInvoker CompileTypedInvoker(MethodInfo method)
    {
        // Parameters of the generated lambda, matching the SubscriptionInvoker delegate signature
        // exactly: (object? target, object? parameter, CancellationToken cancellationToken).
        var targetParam = Expression.Parameter(typeof(object), "target");
        var dataParam = Expression.Parameter(typeof(object), "parameter");
        var ctParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        // For instance methods, cast the incoming object? target to the declaring type so the
        // expression tree can emit a direct virtual/instance call. Static methods pass null.
        Expression? instance = method.IsStatic
            ? null
            : Expression.Convert(targetParam, method.DeclaringType!);

        // Map each formal parameter of the target method to one of the lambda inputs:
        //  - CancellationToken parameters bind to ctParam directly (no boxing).
        //  - Everything else binds to dataParam with an unbox/cast to the expected type.
        var parameters = method.GetParameters();
        var args = new Expression[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var pt = parameters[i].ParameterType;

            args[i] = pt == typeof(CancellationToken)
                ? ctParam
                : Expression.Convert(dataParam, pt);
        }

        // The actual method invocation node. For static methods 'instance' is null which is the
        // correct shape Expression.Call expects.
        Expression call = Expression.Call(instance, method, args);

        // Normalize the return so the compiled delegate always yields a ValueTask, matching
        // SubscriptionInvoker. This keeps the publish loop branch-free at runtime.
        Expression body;
        var returnType = method.ReturnType;

        if (returnType == typeof(ValueTask))
        {
            // Already a ValueTask — return as-is. No allocation, no wrapping.
            body = call;
        }
        else if (returnType == typeof(Task))
        {
            // Wrap Task in a ValueTask via its public constructor. The struct wrapper avoids the
            // boxing that 'object'-returning reflection paths would cause.
            body = Expression.New(ValueTaskFromTaskCtor, call);
        }
        else if (typeof(Task).IsAssignableFrom(returnType))
        {
            // Covers Task<T> and other Task-derived types: cast to Task then wrap in ValueTask.
            body = Expression.New(ValueTaskFromTaskCtor, Expression.Convert(call, typeof(Task)));
        }
        else
        {
            // Synchronous handler (void or non-Task return). Evaluate the call for its side effects,
            // discard the result, then yield the cached ValueTask.CompletedTask singleton.
            body = Expression.Block(call, CompletedTaskExpression);
        }

        // Compile once; the resulting delegate is cached in MethodDescriptor.Invoker and reused for
        // every publish for every subscriber that shares this MethodInfo.
        return Expression.Lambda<SubscriptionInvoker>(body, targetParam, dataParam, ctParam).Compile();
    }

    /// <summary>
    /// AOT-safe fallback used when <see cref="CompileTypedInvoker"/> cannot run (for example when the
    /// host disallows dynamic code generation). Uses <see cref="MethodInvoker"/>, which is faster than
    /// <see cref="MethodBase.Invoke(object, object[])"/> and uses span-based overloads that avoid
    /// allocating an <c>object[]</c> argument array per call.
    /// </summary>
    /// <param name="method">The handler method to wrap.</param>
    /// <returns>A reflection-backed delegate that invokes <paramref name="method"/> and normalizes its result to a <see cref="ValueTask"/>.</returns>
    private static SubscriptionInvoker CreateFallbackInvoker(MethodInfo method)
    {
        // MethodInvoker is created once per MethodInfo and cached on the heap inside the closure.
        // It performs an internal one-time setup so each Invoke call is a near-direct dispatch.
        var invoker = MethodInvoker.Create(method);
        var parameters = method.GetParameters();

        // Recompute the signature shape locally so the closure does not need to reach back into
        // the descriptor's mutable-looking fields, and so this method can be used standalone.
        var supportCancellation = parameters.Length >= 1
            && parameters[^1].ParameterType == typeof(CancellationToken);

        var supportParameter = (parameters.Length == 1 && !supportCancellation)
            || (parameters.Length == 2 && supportCancellation);

        // Capture the mode as a local so the returned delegate uses a single switch over a byte
        // value rather than re-inspecting reflection metadata on every invocation.
        var mode = (InvocationMode)((supportParameter ? 1 : 0) | (supportCancellation ? 2 : 0));

        return (object? target, object? parameter, CancellationToken cancellationToken) =>
        {
            // Dispatch to the correct MethodInvoker overload based on the cached signature shape.
            // Each overload accepts arguments by value (using span-backed overloads internally) so
            // no params array is allocated on the hot path.
            var result = mode switch
            {
                InvocationMode.None => invoker.Invoke(target),
                InvocationMode.Parameter => invoker.Invoke(target, parameter),
                // CancellationToken must be boxed here because MethodInvoker.Invoke takes object?;
                // this is unavoidable on the reflection path but only happens on AOT fallback.
                InvocationMode.Cancellation => invoker.Invoke(target, cancellationToken),
                InvocationMode.ParameterAndCancellation => invoker.Invoke(target, parameter, cancellationToken),
                _ => null,
            };

            // Normalize the boxed return value to a ValueTask so callers always observe a single
            // return type. Pattern-matching avoids casts and keeps null/sync handlers allocation-free
            // by returning the cached ValueTask.CompletedTask singleton.
            return result switch
            {
                ValueTask valueTask => valueTask,
                Task task => new ValueTask(task),
                _ => ValueTask.CompletedTask,
            };
        };
    }
}
