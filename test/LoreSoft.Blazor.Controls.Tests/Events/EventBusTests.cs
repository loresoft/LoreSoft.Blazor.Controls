using LoreSoft.Blazor.Controls.Events;

namespace LoreSoft.Blazor.Controls.Tests.Events;

public class EventBusTests
{
    #region Subscribe Tests

    [Fact]
    public void Subscribe_WithFunc_AddsSubscriber()
    {
        // Arrange
        var eventBus = new EventBus();

        Func<TestEvent, ValueTask> handler = (e) => ValueTask.CompletedTask;

        // Act
        eventBus.Subscribe(handler);

        // Assert - PublishAsync should not throw
        Assert.NotNull(eventBus);
    }

    [Fact]
    public void Subscribe_WithCancellationTokenFunc_AddsSubscriber()
    {
        // Arrange
        var eventBus = new EventBus();

        Func<TestEvent, CancellationToken, ValueTask> handler = (e, ct) => ValueTask.CompletedTask;

        // Act
        eventBus.Subscribe(handler);

        // Assert - PublishAsync should not throw
        Assert.NotNull(eventBus);
    }

    [Fact]
    public void Subscribe_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var eventBus = new EventBus();

        Func<TestEvent, ValueTask> handler = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => eventBus.Subscribe(handler));
    }

    [Fact]
    public void Subscribe_WithNullCancellationTokenHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var eventBus = new EventBus();

        Func<TestEvent, CancellationToken, ValueTask> handler = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => eventBus.Subscribe(handler));
    }

    [Fact]
    public async Task Subscribe_MultipleHandlersForSameEvent_BothReceiveEvents()
    {
        // Arrange
        var eventBus = new EventBus();

        var handler1Called = false;
        var handler2Called = false;

        Func<TestEvent, ValueTask> handler1 = (e) => { handler1Called = true; return ValueTask.CompletedTask; };
        Func<TestEvent, ValueTask> handler2 = (e) => { handler2Called = true; return ValueTask.CompletedTask; };

        eventBus.Subscribe(handler1);
        eventBus.Subscribe(handler2);

        // Act
        var testEvent = new TestEvent { Message = "test" };

        await eventBus.PublishAsync(testEvent);

        // Assert
        Assert.True(handler1Called);
        Assert.True(handler2Called);
    }

    [Fact]
    public async Task Subscribe_DifferentEventTypes_MaintainsSeparateSubscriptions()
    {
        // Arrange
        var eventBus = new EventBus();

        var event1Received = false;
        var event2Received = false;

        Func<TestEvent, ValueTask> handler1 = (e) => { event1Received = true; return ValueTask.CompletedTask; };
        Func<OtherEvent, ValueTask> handler2 = (e) => { event2Received = true; return ValueTask.CompletedTask; };

        eventBus.Subscribe(handler1);
        eventBus.Subscribe(handler2);

        // Act
        await eventBus.PublishAsync(new TestEvent { Message = "test" });

        // Assert
        Assert.True(event1Received);
        Assert.False(event2Received);
    }

    #endregion

    #region PublishAsync Tests

    [Fact]
    public async Task PublishAsync_WithSubscriber_InvokesHandler()
    {
        // Arrange
        var eventBus = new EventBus();

        TestEvent? receivedEvent = null;

        Func<TestEvent, ValueTask> handler = (e) => { receivedEvent = e; return ValueTask.CompletedTask; };

        eventBus.Subscribe(handler);

        var testEvent = new TestEvent { Message = "test message", Value = 42 };

        // Act
        await eventBus.PublishAsync(testEvent);

        // Assert
        Assert.NotNull(receivedEvent);
        Assert.Equal("test message", receivedEvent.Message);
        Assert.Equal(42, receivedEvent.Value);
    }

    [Fact]
    public async Task PublishAsync_WithCancellationTokenHandler_PassesToken()
    {
        // Arrange
        var eventBus = new EventBus();

        CancellationToken receivedToken = default;

        Func<TestEvent, CancellationToken, ValueTask> handler = (e, ct) =>
        {
            receivedToken = ct;
            return ValueTask.CompletedTask;
        };

        eventBus.Subscribe(handler);

        var cts = new CancellationTokenSource();

        // Act
        await eventBus.PublishAsync(new TestEvent { Message = "test" }, cts.Token);

        // Assert
        Assert.Equal(cts.Token, receivedToken);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleHandlers_InvokesAll()
    {
        // Arrange
        var eventBus = new EventBus();

        var handler1Count = 0;
        var handler2Count = 0;

        Func<TestEvent, ValueTask> handler1 = (e) => { handler1Count++; return ValueTask.CompletedTask; };
        Func<TestEvent, ValueTask> handler2 = (e) => { handler2Count++; return ValueTask.CompletedTask; };

        eventBus.Subscribe(handler1);
        eventBus.Subscribe(handler2);

        // Act
        await eventBus.PublishAsync(new TestEvent { Message = "test" });

        // Assert
        Assert.Equal(1, handler1Count);
        Assert.Equal(1, handler2Count);
    }

    [Fact]
    public async Task PublishAsync_WithAsyncHandler_AwaitsCompletion()
    {
        // Arrange
        var eventBus = new EventBus();

        var completed = false;
        Func<TestEvent, ValueTask> handler = async (e) =>
        {
            await Task.Delay(10);
            completed = true;
        };

        eventBus.Subscribe(handler);

        // Act
        await eventBus.PublishAsync(new TestEvent { Message = "test" });

        // Assert
        Assert.True(completed);
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_CompletesSuccessfully()
    {
        // Arrange
        var eventBus = new EventBus();

        // Act & Assert - should not throw
        await eventBus.PublishAsync(new TestEvent { Message = "test" });
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var eventBus = new EventBus();

        TestEvent nullEvent = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => eventBus.PublishAsync(nullEvent).AsTask());
    }

    [Fact]
    public async Task PublishAsync_MultipleTimes_InvokesHandlerMultipleTimes()
    {
        // Arrange
        var eventBus = new EventBus();

        var callCount = 0;
        Func<TestEvent, ValueTask> handler = (e) => { callCount++; return ValueTask.CompletedTask; };

        eventBus.Subscribe(handler);

        // Act
        await eventBus.PublishAsync(new TestEvent { Message = "test1" });
        await eventBus.PublishAsync(new TestEvent { Message = "test2" });
        await eventBus.PublishAsync(new TestEvent { Message = "test3" });

        // Assert
        Assert.Equal(3, callCount);
    }

    #endregion

    #region Unsubscribe Tests

    [Fact]
    public async Task Unsubscribe_WithFunc_RemovesHandler()
    {
        // Arrange
        var eventBus = new EventBus();

        var callCount = 0;
        Func<TestEvent, ValueTask> handler = (e) => { callCount++; return ValueTask.CompletedTask; };

        eventBus.Subscribe(handler);

        // Act
        eventBus.Unsubscribe(handler);

        await eventBus.PublishAsync(new TestEvent { Message = "test" });

        // Assert
        Assert.Equal(0, callCount);
    }

    [Fact]
    public async Task Unsubscribe_WithCancellationTokenFunc_RemovesHandler()
    {
        // Arrange
        var eventBus = new EventBus();

        var callCount = 0;
        Func<TestEvent, CancellationToken, ValueTask> handler = (e, ct) => { callCount++; return ValueTask.CompletedTask; };

        eventBus.Subscribe(handler);

        // Act
        eventBus.Unsubscribe(handler);

        await eventBus.PublishAsync(new TestEvent { Message = "test" });

        // Assert
        Assert.Equal(0, callCount);
    }

    [Fact]
    public void Unsubscribe_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var eventBus = new EventBus();

        Func<TestEvent, ValueTask> handler = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => eventBus.Unsubscribe(handler));
    }

    [Fact]
    public void Unsubscribe_WithNullCancellationTokenHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var eventBus = new EventBus();

        Func<TestEvent, CancellationToken, ValueTask> handler = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => eventBus.Unsubscribe(handler));
    }

    [Fact]
    public async Task Unsubscribe_OneHandlerOfMultiple_OnlyRemovedHandlerNotInvoked()
    {
        // Arrange
        var eventBus = new EventBus();

        var handler1Count = 0;
        var handler2Count = 0;

        Func<TestEvent, ValueTask> handler1 = (e) => { handler1Count++; return ValueTask.CompletedTask; };
        Func<TestEvent, ValueTask> handler2 = (e) => { handler2Count++; return ValueTask.CompletedTask; };

        eventBus.Subscribe(handler1);
        eventBus.Subscribe(handler2);

        // Act
        eventBus.Unsubscribe(handler1);

        await eventBus.PublishAsync(new TestEvent { Message = "test" });

        // Assert
        Assert.Equal(0, handler1Count);
        Assert.Equal(1, handler2Count);
    }

    [Fact]
    public async Task Unsubscribe_BySubscriberObject_RemovesAllHandlersForThatSubscriber()
    {
        // Arrange
        var eventBus = new EventBus();

        var subscriber = new EventSubscriber();

        eventBus.Subscribe<TestEvent>(subscriber.HandleTestEvent);
        eventBus.Subscribe<OtherEvent>(subscriber.HandleOtherEvent);

        // Act
        eventBus.Unsubscribe(subscriber);

        await eventBus.PublishAsync(new TestEvent { Message = "test" });
        await eventBus.PublishAsync(new OtherEvent { Data = "test" });

        // Assert
        Assert.Equal(0, subscriber.TestEventCount);
        Assert.Equal(0, subscriber.OtherEventCount);
    }

    [Fact]
    public void Unsubscribe_ByNullSubscriber_ThrowsArgumentNullException()
    {
        // Arrange
        var eventBus = new EventBus();

        object subscriber = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => eventBus.Unsubscribe(subscriber));
    }

    [Fact]
    public void Unsubscribe_NonExistentHandler_DoesNotThrow()
    {
        // Arrange
        var eventBus = new EventBus();

        Func<TestEvent, ValueTask> handler = (e) => ValueTask.CompletedTask;

        // Act & Assert - should not throw
        eventBus.Unsubscribe(handler);
    }

    #endregion

    #region Garbage Collection Tests

    [Fact]
    public async Task PublishAsync_WithCollectedSubscriber_DoesNotInvokeHandler()
    {
        // Arrange
        var eventBus = new EventBus();

        var callTracker = new CallTracker();

        SubscribeWeakHandler(eventBus, callTracker);

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Act
        await eventBus.PublishAsync(new TestEvent { Message = "test" });

        // Assert
        Assert.Equal(0, callTracker.CallCount);
    }

    [Fact]
    public async Task PublishAsync_WithPartiallyCollectedSubscribers_OnlyInvokesLiveHandlers()
    {
        // Arrange
        var eventBus = new EventBus();

        var strongCallTracker = new CallTracker();
        var weakCallTracker = new CallTracker();

        // Subscribe a strong reference handler (won't be collected)
        var strongSubscriber = new WeakEventSubscriber(strongCallTracker);
        eventBus.Subscribe<TestEvent>(strongSubscriber.HandleEvent);

        // Subscribe a weak reference handler (will be collected)
        SubscribeWeakHandler(eventBus, weakCallTracker);

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Act
        await eventBus.PublishAsync(new TestEvent { Message = "test" });

        // Assert
        Assert.Equal(1, strongCallTracker.CallCount); // Strong reference handler called
        Assert.Equal(0, weakCallTracker.CallCount);   // Weak reference handler not called
    }

    [Fact]
    public async Task PublishAsync_WithMultipleCollectedSubscribers_DoesNotInvokeAnyHandler()
    {
        // Arrange
        var eventBus = new EventBus();

        var callTracker1 = new CallTracker();
        var callTracker2 = new CallTracker();
        var callTracker3 = new CallTracker();

        SubscribeWeakHandler(eventBus, callTracker1);
        SubscribeWeakHandler(eventBus, callTracker2);
        SubscribeWeakHandler(eventBus, callTracker3);

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Act
        await eventBus.PublishAsync(new TestEvent { Message = "test" });

        // Assert
        Assert.Equal(0, callTracker1.CallCount);
        Assert.Equal(0, callTracker2.CallCount);
        Assert.Equal(0, callTracker3.CallCount);
    }

    [Fact]
    public async Task Subscribe_AfterGarbageCollection_NewHandlerStillWorks()
    {
        // Arrange
        var eventBus = new EventBus();

        var weakCallTracker = new CallTracker();
        var newCallTracker = new CallTracker();

        // Subscribe weak handler
        SubscribeWeakHandler(eventBus, weakCallTracker);

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Subscribe new handler after GC
        var newSubscriber = new WeakEventSubscriber(newCallTracker);
        eventBus.Subscribe<TestEvent>(newSubscriber.HandleEvent);

        // Act
        await eventBus.PublishAsync(new TestEvent { Message = "test" });

        // Assert
        Assert.Equal(0, weakCallTracker.CallCount);  // Old handler not invoked
        Assert.Equal(1, newCallTracker.CallCount);   // New handler invoked
    }

    [Fact]
    public async Task PublishAsync_WithCollectedSubscribersOfDifferentEventTypes_CleansUpCorrectly()
    {
        // Arrange
        var eventBus = new EventBus();

        var testEventTracker = new CallTracker();
        var otherEventTracker = new CallTracker();

        // Subscribe weak handlers for different event types
        SubscribeWeakHandler(eventBus, testEventTracker);
        SubscribeWeakOtherEventHandler(eventBus, otherEventTracker);

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Act
        await eventBus.PublishAsync(new TestEvent { Message = "test" });
        await eventBus.PublishAsync(new OtherEvent { Data = "other" });

        // Assert
        Assert.Equal(0, testEventTracker.CallCount);
        Assert.Equal(0, otherEventTracker.CallCount);
    }

    [Fact]
    public async Task PublishAsync_WithMixedStrongAndWeakReferences_BehavesCorrectly()
    {
        // Arrange
        var eventBus = new EventBus();

        var strongTracker1 = new CallTracker();
        var weakTracker = new CallTracker();
        var strongTracker2 = new CallTracker();

        // Subscribe: strong, weak, strong
        var strong1 = new WeakEventSubscriber(strongTracker1);
        eventBus.Subscribe<TestEvent>(strong1.HandleEvent);

        SubscribeWeakHandler(eventBus, weakTracker);

        var strong2 = new WeakEventSubscriber(strongTracker2);
        eventBus.Subscribe<TestEvent>(strong2.HandleEvent);

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Act
        await eventBus.PublishAsync(new TestEvent { Message = "test" });

        // Assert
        Assert.Equal(1, strongTracker1.CallCount); // Strong reference invoked
        Assert.Equal(0, weakTracker.CallCount);    // Weak reference not invoked
        Assert.Equal(1, strongTracker2.CallCount); // Strong reference invoked
    }

    [Fact]
    public async Task Unsubscribe_BeforeGarbageCollection_PreventsLeaks()
    {
        // Arrange
        var eventBus = new EventBus();

        var subscriber = new WeakEventSubscriber(new CallTracker());

        eventBus.Subscribe<TestEvent>(subscriber.HandleEvent);

        // Act - Unsubscribe before GC
        eventBus.Unsubscribe(subscriber);

        // Allow GC
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Assert - Publishing should complete without error
        await eventBus.PublishAsync(new TestEvent { Message = "test" });

        Assert.Equal(0, subscriber._tracker.CallCount);
    }

    [Fact]
    public async Task PublishAsync_WithCollectedCancellationTokenHandler_DoesNotInvokeHandler()
    {
        // Arrange
        var eventBus = new EventBus();
        var callTracker = new CallTracker();

        SubscribeWeakCancellationTokenHandler(eventBus, callTracker);

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var cts = new CancellationTokenSource();

        // Act
        await eventBus.PublishAsync(new TestEvent { Message = "test" }, cts.Token);

        // Assert
        Assert.Equal(0, callTracker.CallCount);
    }

    #endregion

    #region Multiple Event Types Tests

    [Fact]
    public async Task PublishAsync_WithMultipleEventTypes_OnlyInvokesMatchingHandlers()
    {
        // Arrange
        var eventBus = new EventBus();

        var testEventCount = 0;
        var otherEventCount = 0;

        Func<TestEvent, ValueTask> testHandler = (e) => { testEventCount++; return ValueTask.CompletedTask; };
        Func<OtherEvent, ValueTask> otherHandler = (e) => { otherEventCount++; return ValueTask.CompletedTask; };

        eventBus.Subscribe(testHandler);
        eventBus.Subscribe(otherHandler);

        // Act
        await eventBus.PublishAsync(new TestEvent { Message = "test" });
        await eventBus.PublishAsync(new OtherEvent { Data = "other" });

        // Assert
        Assert.Equal(1, testEventCount);
        Assert.Equal(1, otherEventCount);
    }

    [Fact]
    public async Task Unsubscribe_FromOneEventType_DoesNotAffectOtherEventTypes()
    {
        // Arrange
        var eventBus = new EventBus();

        var testEventCount = 0;
        var otherEventCount = 0;

        Func<TestEvent, ValueTask> testHandler = (e) => { testEventCount++; return ValueTask.CompletedTask; };
        Func<OtherEvent, ValueTask> otherHandler = (e) => { otherEventCount++; return ValueTask.CompletedTask; };

        eventBus.Subscribe(testHandler);
        eventBus.Subscribe(otherHandler);

        // Act
        eventBus.Unsubscribe(testHandler);

        await eventBus.PublishAsync(new TestEvent { Message = "test" });
        await eventBus.PublishAsync(new OtherEvent { Data = "other" });

        // Assert
        Assert.Equal(0, testEventCount);
        Assert.Equal(1, otherEventCount);
    }

    #endregion

    #region Helper Methods and Classes

    private static void SubscribeWeakHandler(EventBus eventBus, CallTracker tracker)
    {
        var subscriber = new WeakEventSubscriber(tracker);
        eventBus.Subscribe<TestEvent>(subscriber.HandleEvent);
    }

    private static void SubscribeWeakOtherEventHandler(EventBus eventBus, CallTracker tracker)
    {
        var subscriber = new WeakOtherEventSubscriber(tracker);
        eventBus.Subscribe<OtherEvent>(subscriber.HandleEvent);
    }

    private static void SubscribeWeakCancellationTokenHandler(EventBus eventBus, CallTracker tracker)
    {
        var subscriber = new WeakCancellationTokenSubscriber(tracker);
        eventBus.Subscribe<TestEvent>(subscriber.HandleEvent);
    }

    private class CallTracker
    {
        public int CallCount { get; private set; }

        public void IncrementCallCount() => CallCount++;
    }

    private class WeakEventSubscriber
    {
        internal readonly CallTracker _tracker;

        public WeakEventSubscriber(CallTracker tracker)
        {
            _tracker = tracker;
        }

        public ValueTask HandleEvent(TestEvent e)
        {
            _tracker.IncrementCallCount();
            return ValueTask.CompletedTask;
        }
    }

    private class WeakOtherEventSubscriber
    {
        private readonly CallTracker _tracker;

        public WeakOtherEventSubscriber(CallTracker tracker)
        {
            _tracker = tracker;
        }

        public ValueTask HandleEvent(OtherEvent e)
        {
            _tracker.IncrementCallCount();
            return ValueTask.CompletedTask;
        }
    }

    private class WeakCancellationTokenSubscriber
    {
        private readonly CallTracker _tracker;

        public WeakCancellationTokenSubscriber(CallTracker tracker)
        {
            _tracker = tracker;
        }

        public ValueTask HandleEvent(TestEvent e, CancellationToken ct)
        {
            _tracker.IncrementCallCount();
            return ValueTask.CompletedTask;
        }
    }

    private class EventSubscriber
    {
        public int TestEventCount { get; private set; }
        public int OtherEventCount { get; private set; }

        public ValueTask HandleTestEvent(TestEvent e)
        {
            TestEventCount++;
            return ValueTask.CompletedTask;
        }

        public ValueTask HandleOtherEvent(OtherEvent e)
        {
            OtherEventCount++;
            return ValueTask.CompletedTask;
        }
    }

    private class TestEvent
    {
        public string Message { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private class OtherEvent
    {
        public string Data { get; set; } = string.Empty;
    }

    #endregion
}
