using System.Buffers;

namespace LoreSoft.Blazor.Controls.Events;

/// <summary>
/// Provides a base implementation for weak event patterns that automatically clean up garbage collected subscribers.
/// </summary>
/// <remarks>
/// <para>
/// This class uses weak references to store event handlers, allowing subscribers to be garbage collected
/// without explicitly unsubscribing. This prevents common memory leaks associated with event handlers.
/// </para>
/// <para>
/// The subscriber list is stored as an immutable <see cref="Snapshot"/> pair of
/// (<see cref="WeakSubscription"/> array, live count). Subscribe uses geometric growth: when the
/// existing array has spare capacity the new handler is written into the slot just past the published
/// count and a new snapshot is published; only when capacity is exhausted does a doubled-size array
/// get allocated. This collapses the per-subscribe array allocation from O(N²) total bytes to O(N)
/// amortized while keeping the publish path allocation-free.
/// </para>
/// <para>
/// Writers serialize on <c>_writerLock</c> (a cheap uncontended monitor) so they never race on the
/// shared array's free slot. Readers (publish) never take the lock — they read the snapshot via
/// <see cref="Volatile"/>.<c>Read</c> and iterate <c>[0, Count)</c>. The release semantics of the
/// lock release on writers and the acquire semantics of <see cref="Volatile"/>.<c>Read</c> on
/// readers ensure that any element write at <c>Items[Count]</c> happens-before the snapshot
/// publication that exposes it.
/// </para>
/// <para>
/// Handlers are invoked asynchronously and concurrently. Dead handlers (whose targets have been garbage collected)
/// are automatically cleaned up by a background prune scheduled on the thread pool when the dead count
/// crosses a threshold.
/// </para>
/// </remarks>
public class WeakEventBase
{
    /// <summary>
    /// Number of dead handlers tolerated in a publish snapshot before a background prune is scheduled.
    /// </summary>
    private const int AsyncPruneDeadHandlerThreshold = 8;

    /// <summary>
    /// Initial array capacity allocated on the first subscribe. Small enough to avoid wasting bytes
    /// for the common single-subscriber case (Publish ignores the unused tail), large enough to
    /// amortize subsequent growths.
    /// </summary>
    private const int InitialCapacity = 4;

    /// <summary>
    /// Static thread-pool callback used by <see cref="SchedulePrune"/> to avoid allocating a closure
    /// on every prune request.
    /// </summary>
    private static readonly Action<WeakEventBase> PruneCallback = static state => state.PruneDeadHandlers();

    /// <summary>
    /// Immutable snapshot exposing the (array, count) pair Publish reads lock-free. The array may
    /// contain unused tail slots; readers must iterate only <c>[0, Count)</c>. Replaced via
    /// <see cref="Volatile.Write{T}(ref T, T)"/> under <c>_writerLock</c>; read via
    /// <see cref="Volatile"/>.<c>Read</c> on the publish hot path.
    /// </summary>
    private Snapshot _snapshot = Snapshot.Empty;

#if NET9_0_OR_GREATER
    /// <summary>
    /// Serializes writers so that mutations to the shared <see cref="Snapshot.Items"/> tail slot
    /// (during in-place geometric growth) are safe. Readers never acquire this lock.
    /// </summary>
    private readonly Lock _writerLock = new();
#else
    /// <summary>
    /// Serializes writers so that mutations to the shared <see cref="Snapshot.Items"/> tail slot
    /// (during in-place geometric growth) are safe. Readers never acquire this lock.
    /// </summary>
    private readonly object _writerLock = new();
#endif

    /// <summary>
    /// Flag (0 or 1) tracking whether a prune work item has already been queued, ensuring at most one
    /// outstanding prune per instance.
    /// </summary>
    private int _pruneScheduled;


    /// <summary>
    /// Gets the count of active subscribers that have not been garbage collected.
    /// </summary>
    /// <returns>The number of alive subscribers.</returns>
    /// <remarks>
    /// Reads the subscriber snapshot lock-free via <see cref="Volatile"/>.<c>Read</c>. The count
    /// reflects the number of handlers whose target objects are alive at the moment of the call.
    /// </remarks>
    public int SubscriberCount()
    {
        var snapshot = Volatile.Read(ref _snapshot);
        var items = snapshot.Items;
        int total = snapshot.Count;

        int count = 0;
        for (int i = 0; i < total; i++)
        {
            if (items[i].IsAlive)
                count++;
        }

        return count;
    }

    /// <summary>
    /// Removes all event handlers associated with the specified subscriber object.
    /// </summary>
    /// <param name="subscriber">The subscriber object whose handlers should be removed. Cannot be <see langword="null"/>.</param>
    /// <returns>The remaining number of handlers after removal.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="subscriber"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method is typically called when the subscriber is being disposed to ensure immediate cleanup
    /// rather than waiting for garbage collection.
    /// </para>
    /// <para>
    /// Uses reference equality to match handlers by their target object. The handler list is updated
    /// using copy-on-write, so the new array is published atomically.
    /// </para>
    /// </remarks>
    public int Unsubscribe(object subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        // Writer-side mutation requires the writer lock so concurrent Subscribe/Unsubscribe/Prune
        // calls cannot race on the snapshot's tail capacity. Publish never takes this lock.
        lock (_writerLock)
        {
            var current = _snapshot;
            var currentItems = current.Items;
            int currentCount = current.Count;
            int keep = 0;

            // First pass: count survivors. Since we are already paying for a full scan and a
            // replacement-array allocation, we opportunistically also discard dead handlers —
            // they can never be invoked and would otherwise wait for the background prune. A
            // survivor must both not match the subscriber AND still be alive.
            for (int i = 0; i < currentCount; i++)
            {
                var entry = currentItems[i];
                if (!entry.IsTargetMatch(subscriber) && entry.IsAlive)
                    keep++;
            }

            // Nothing changed: no matches AND no dead handlers found. Leave the snapshot in place.
            if (keep == currentCount)
                return currentCount;

            // Nothing survives. Publish the empty snapshot singleton.
            if (keep == 0)
            {
                Volatile.Write(ref _snapshot, Snapshot.Empty);
                return 0;
            }

            // Second pass: copy survivors into a precisely sized array. Unsubscribe always shrinks
            // to an exact-fit array (rather than reusing the oversized buffer) so a concurrent
            // publisher iterating the previous snapshot continues to see its live elements
            // unchanged — we never overwrite slots that may still be observed.
            var next = new WeakSubscription[keep];
            int j = 0;

            for (int i = 0; i < currentCount; i++)
            {
                var entry = currentItems[i];
                if (!entry.IsTargetMatch(subscriber) && entry.IsAlive)
                    next[j++] = entry;
            }

            Volatile.Write(ref _snapshot, new Snapshot(next, keep));
            return keep;
        }
    }

    /// <summary>
    /// Subscribes a delegate as a weak event handler.
    /// </summary>
    /// <param name="handler">The delegate to subscribe. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// The handler is stored as a weak reference, allowing the target object to be garbage collected
    /// without explicitly unsubscribing.
    /// </para>
    /// <para>
    /// Subscribe uses geometric growth. When the published snapshot has spare capacity the new handler
    /// is written into the slot just past the live count and a new <see cref="Snapshot"/> exposing
    /// <c>count + 1</c> elements is published. Only when capacity is exhausted does a doubled-size
    /// array get allocated and copied — turning the per-subscribe cost from <c>O(N)</c> bytes (and
    /// <c>O(N²)</c> total across N subscribes) into amortized <c>O(1)</c> bytes per subscribe.
    /// </para>
    /// <para>
    /// Duplicate subscriptions are not prevented - the same delegate can be subscribed multiple times
    /// and will be invoked once for each subscription.
    /// </para>
    /// </remarks>
    protected void SubscribeCore(Delegate handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        // Build the WeakSubscription outside the critical section. The constructor performs a
        // dictionary lookup (and on first sight an Expression.Compile) which we deliberately keep
        // outside the writer lock so concurrent subscribers do not serialize on reflection cost.
        var subscription = new WeakSubscription(handler);

        lock (_writerLock)
        {
            var current = _snapshot;
            var currentItems = current.Items;
            int currentCount = current.Count;

            if (currentCount < currentItems.Length)
            {
                // Fast path: the array has spare tail capacity. Write into the slot just past the
                // published count — this slot is currently invisible to Publish (which iterates
                // [0, currentCount)). The Volatile.Write below uses release semantics, so the
                // element write happens-before any publisher's Volatile.Read of the new snapshot.
                currentItems[currentCount] = subscription;
                Volatile.Write(ref _snapshot, new Snapshot(currentItems, currentCount + 1));
                return;
            }

            // Grow path: capacity exhausted. Allocate a doubled (or initial) buffer, copy live
            // elements, append the new subscription, and publish. This is the only allocation that
            // scales with subscriber count, and it amortizes to O(1) per subscribe.
            int newCapacity = currentItems.Length == 0 ? InitialCapacity : currentItems.Length * 2;
            var next = new WeakSubscription[newCapacity];

            if (currentCount > 0)
                Array.Copy(currentItems, next, currentCount);

            next[currentCount] = subscription;
            Volatile.Write(ref _snapshot, new Snapshot(next, currentCount + 1));
        }
    }

    /// <summary>
    /// Unsubscribes a specific delegate from the event.
    /// </summary>
    /// <param name="handler">The delegate to unsubscribe. Cannot be <see langword="null"/>.</param>
    /// <returns>The remaining number of handlers after removal.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// Removes all instances of the specified delegate. If the delegate was subscribed multiple times,
    /// all subscriptions are removed.
    /// </para>
    /// <para>
    /// The match is based on both the target object and the method. Two delegates with the same method
    /// but different targets are not considered equal.
    /// </para>
    /// <para>
    /// Uses copy-on-write to publish the updated array atomically.
    /// </para>
    /// </remarks>
    protected int UnsubscribeCore(Delegate handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_writerLock)
        {
            var current = _snapshot;
            var currentItems = current.Items;
            int currentCount = current.Count;
            int keep = 0;

            // First pass: count survivors. Since we are already paying for a full scan and a
            // replacement-array allocation, also discard dead handlers — they can never be
            // invoked and would otherwise wait for the background prune. A survivor must both
            // not match the handler AND still be alive.
            for (int i = 0; i < currentCount; i++)
            {
                var entry = currentItems[i];
                if (!entry.IsDelegateMatch(handler) && entry.IsAlive)
                    keep++;
            }

            // Nothing changed: no matches AND no dead handlers found.
            if (keep == currentCount)
                return currentCount;

            // Nothing survives. Publish the empty snapshot singleton.
            if (keep == 0)
            {
                Volatile.Write(ref _snapshot, Snapshot.Empty);
                return 0;
            }

            // Exact-fit copy. Like Unsubscribe(object), we never mutate the live region of the
            // currently published array — concurrent publishers may still be iterating it.
            var next = new WeakSubscription[keep];
            int j = 0;

            for (int i = 0; i < currentCount; i++)
            {
                var entry = currentItems[i];
                if (!entry.IsDelegateMatch(handler) && entry.IsAlive)
                    next[j++] = entry;
            }

            Volatile.Write(ref _snapshot, new Snapshot(next, keep));
            return keep;
        }
    }

    /// <summary>
    /// Publishes an event to all active subscribers by invoking their handlers asynchronously and concurrently.
    /// </summary>
    /// <param name="eventData">The event data to pass to each event handler. Can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation.
    /// This token is passed to handlers that support cancellation in their method signature.
    /// </param>
    /// <returns>A <see cref="ValueTask"/> that completes when all handlers have been invoked.</returns>
    /// <remarks>
    /// <para>
    /// Reads the subscriber snapshot lock-free; subscribe and unsubscribe operations may run concurrently
    /// without blocking publication.
    /// </para>
    /// <para>
    /// Dead handlers (whose targets have been garbage collected) are skipped during invocation. When
    /// the dead-handler count crosses a threshold a background prune is scheduled on the thread pool.
    /// </para>
    /// <para>
    /// All handlers are invoked concurrently using <see cref="ValueTask"/>. This method awaits all
    /// of them to complete before returning. Uses <see cref="ArrayPool{T}"/> to avoid allocating a
    /// task tracking array on each invocation.
    /// </para>
    /// <para>
    /// If a handler throws an exception, it will propagate to the caller. Handlers that have not yet
    /// been started might not be invoked after a synchronous exception.
    /// </para>
    /// </remarks>
    protected async ValueTask PublishCoreAsync(object? eventData, CancellationToken cancellationToken = default)
    {
        // Lock-free read of the immutable subscriber snapshot. Concurrent subscribe/unsubscribe writers
        // may publish a new snapshot while we run, but our local 'items' and 'count' continue to
        // reference the view we observed here, so iteration is always safe.
        //
        // The snapshot exposes (Items, Count); Items may be longer than Count (geometric growth tail).
        // We strictly iterate [0, Count) — slots past Count are writer-owned scratch space.
        var snapshot = Volatile.Read(ref _snapshot);
        var items = snapshot.Items;
        int count = snapshot.Count;
        if (count == 0)
            return;

        // Single-handler fast path. The overwhelming majority of weak events in UI code carry just one
        // subscriber, so we skip the ArrayPool rental, the dead-count bookkeeping, and the start/await
        // split entirely. TryInvoke returns false when the weak target has already been collected, in
        // which case there is simply nothing to await.
        if (count == 1)
        {
            if (items[0].TryInvoke(eventData, cancellationToken, out var single))
                await single.ConfigureAwait(false);

            return;
        }

        // 'tasks' is rented lazily inside the try so a failure before the rental does not require a
        // matching return. activeCount tracks how many slots we have written and is also the upper
        // bound used by both the await loop and the Array.Clear in finally.
        ValueTask[]? tasks = null;
        int activeCount = 0;
        int deadCount = 0;

        try
        {
            // Rent a buffer large enough for every potential live handler. ArrayPool may return a larger
            // array than requested; that is fine because we always index by activeCount, never by Length.
            tasks = ArrayPool<ValueTask>.Shared.Rent(count);

            // Start phase: invoke every handler eagerly so they run concurrently. Each TryInvoke either
            //   - returns true and yields a ValueTask we will await later, or
            //   - returns false because the weak target was collected; we count that for the prune heuristic.
            // We deliberately do not await inside the loop so handlers overlap rather than serialize.
            for (int i = 0; i < count; i++)
            {
                if (items[i].TryInvoke(eventData, cancellationToken, out var task))
                    tasks[activeCount++] = task;
                else
                    deadCount++;
            }

            // Cooperative cleanup: if enough handlers were dead, kick off a background prune so future
            // publishes do not keep paying the dead-handler scan cost. SchedulePrune is single-flight
            // and uses a static callback, so this is essentially free when a prune is already pending.
            if (deadCount >= AsyncPruneDeadHandlerThreshold)
                SchedulePrune();

            // Await phase: drain the started ValueTasks in order. Awaiting a completed ValueTask is
            // allocation-free; only handlers that actually went async cost anything beyond the await.
            // If any handler throws, the exception propagates here and the remaining ValueTasks in
            // the rented buffer will still be cleared by the finally block.
            for (int i = 0; i < activeCount; i++)
                await tasks[i].ConfigureAwait(false);
        }
        finally
        {
            if (tasks != null)
            {
                // Clear only the slots we wrote so the pool does not hold onto IValueTaskSource refs
                // that could keep async state machines alive after they have completed. We skip clearing
                // inside ArrayPool.Return because we just did it precisely above.
                Array.Clear(tasks, 0, activeCount);
                ArrayPool<ValueTask>.Shared.Return(tasks, clearArray: false);
            }
        }
    }

    /// <summary>
    /// Queues a single background prune on the thread pool if one is not already pending.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="Interlocked.Exchange(ref int, int)"/> to ensure at most one outstanding prune per
    /// instance, and <see cref="ThreadPool.UnsafeQueueUserWorkItem{TState}(Action{TState}, TState, bool)"/>
    /// with a cached static callback to avoid closure allocation. In Blazor WebAssembly the work item is
    /// dispatched cooperatively on the single-threaded runtime, which is safe by design.
    /// </remarks>
    private void SchedulePrune()
    {
        if (Interlocked.Exchange(ref _pruneScheduled, 1) == 1)
            return;

        ThreadPool.UnsafeQueueUserWorkItem(PruneCallback, this, preferLocal: false);
    }

    /// <summary>
    /// Removes dead (garbage-collected) subscribers from the snapshot using copy-on-write semantics.
    /// </summary>
    /// <remarks>
    /// Runs on the thread pool and coordinates with subscribe/unsubscribe via a lock-free CAS retry loop.
    /// If no handlers need to be removed the existing array is retained. The <c>_pruneScheduled</c> flag is
    /// always cleared in a <see langword="finally"/> block so future dead handlers can trigger another prune.
    /// </remarks>
    private void PruneDeadHandlers()
    {
        try
        {
            lock (_writerLock)
            {
                var current = _snapshot;
                var currentItems = current.Items;
                int currentCount = current.Count;
                int keep = 0;

                // First pass: count survivors so the replacement can be sized exactly.
                for (int i = 0; i < currentCount; i++)
                {
                    if (currentItems[i].IsAlive)
                        keep++;
                }

                // Nothing is dead anymore (e.g. an Unsubscribe ran in the interim).
                if (keep == currentCount)
                    return;

                // Every handler is dead. Publish the empty snapshot singleton.
                if (keep == 0)
                {
                    Volatile.Write(ref _snapshot, Snapshot.Empty);
                    return;
                }

                // Exact-fit copy of live entries. Same reasoning as UnsubscribeCore: we never mutate
                // the currently published array because concurrent publishers may still be iterating it.
                var next = new WeakSubscription[keep];
                int j = 0;

                for (int i = 0; i < currentCount; i++)
                {
                    if (currentItems[i].IsAlive)
                        next[j++] = currentItems[i];
                }

                Volatile.Write(ref _snapshot, new Snapshot(next, keep));
            }
        }
        finally
        {
            // Release the single-flight latch unconditionally so the next dead-handler threshold breach
            // can queue another prune.
            Volatile.Write(ref _pruneScheduled, 0);
        }
    }

    /// <summary>
    /// Immutable view of the subscriber list at a point in time. <see cref="Items"/> may be longer
    /// than <see cref="Count"/>; the tail slots are writer-owned scratch space that publishers must
    /// not touch.
    /// </summary>
    private sealed class Snapshot
    {
        public static readonly Snapshot Empty = new([], 0);

        public readonly WeakSubscription[] Items;
        public readonly int Count;

        public Snapshot(WeakSubscription[] items, int count)
        {
            Items = items;
            Count = count;
        }
    }
}
