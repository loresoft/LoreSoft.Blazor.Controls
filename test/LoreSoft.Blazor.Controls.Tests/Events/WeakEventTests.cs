using LoreSoft.Blazor.Controls.Events;

namespace LoreSoft.Blazor.Controls.Tests.Events;

public class WeakEventTests
{
    #region WeakEvent (Parameterless) Tests

    [Fact]
    public void Subscribe_WithAction_AddsSubscriber()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        var callCount = 0;
        Action handler = () => callCount++;

        // Act
        weakEvent.Subscribe(handler);

        // Assert
        Assert.Equal(1, weakEvent.SubscriberCount());
    }

    [Fact]
    public void Subscribe_WithFunc_AddsSubscriber()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        Func<ValueTask> handler = () => ValueTask.CompletedTask;

        // Act
        weakEvent.Subscribe(handler);

        // Assert
        Assert.Equal(1, weakEvent.SubscriberCount());
    }

    [Fact]
    public void Subscribe_WithCancellationTokenFunc_AddsSubscriber()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        Func<CancellationToken, ValueTask> handler = (ct) => ValueTask.CompletedTask;

        // Act
        weakEvent.Subscribe(handler);

        // Assert
        Assert.Equal(1, weakEvent.SubscriberCount());
    }

    [Fact]
    public void Subscribe_MultipleHandlers_IncreasesCount()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        Action handler1 = () => { };
        Action handler2 = () => { };

        // Act
        weakEvent.Subscribe(handler1);
        weakEvent.Subscribe(handler2);

        // Assert
        Assert.Equal(2, weakEvent.SubscriberCount());
    }

    [Fact]
    public async Task PublishAsync_WithAction_ExecutesHandler()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        var callCount = 0;
        Action handler = () => callCount++;

        weakEvent.Subscribe(handler);

        // Act
        await weakEvent.PublishAsync();

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleHandlers_ExecutesAll()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        var callCount1 = 0;
        var callCount2 = 0;
        Action handler1 = () => callCount1++;
        Action handler2 = () => callCount2++;

        weakEvent.Subscribe(handler1);
        weakEvent.Subscribe(handler2);

        // Act
        await weakEvent.PublishAsync();

        // Assert
        Assert.Equal(1, callCount1);
        Assert.Equal(1, callCount2);
    }

    [Fact]
    public async Task PublishAsync_WithAsyncHandler_AwaitsCompletion()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        var completed = false;
        Func<ValueTask> handler = async () =>
        {
            await Task.Delay(10);
            completed = true;
        };

        weakEvent.Subscribe(handler);

        // Act
        await weakEvent.PublishAsync();

        // Assert
        Assert.True(completed);
    }

    [Fact]
    public async Task PublishAsync_WithCancellationToken_PassesToken()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        CancellationToken receivedToken = default;
        Func<CancellationToken, ValueTask> handler = (ct) =>
        {
            receivedToken = ct;
            return ValueTask.CompletedTask;
        };

        weakEvent.Subscribe(handler);
        var cts = new CancellationTokenSource();

        // Act
        await weakEvent.PublishAsync(cts.Token);

        // Assert
        Assert.Equal(cts.Token, receivedToken);
    }

    [Fact]
    public void Unsubscribe_WithAction_RemovesHandler()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        Action handler = () => { };

        weakEvent.Subscribe(handler);

        // Act
        var remainingCount = weakEvent.Unsubscribe(handler);

        // Assert
        Assert.Equal(0, remainingCount);
        Assert.Equal(0, weakEvent.SubscriberCount());
    }

    [Fact]
    public void Unsubscribe_WithFunc_RemovesHandler()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        Func<ValueTask> handler = () => ValueTask.CompletedTask;

        weakEvent.Subscribe(handler);

        // Act
        var remainingCount = weakEvent.Unsubscribe(handler);

        // Assert
        Assert.Equal(0, remainingCount);
        Assert.Equal(0, weakEvent.SubscriberCount());
    }

    [Fact]
    public void Unsubscribe_WithCancellationTokenFunc_RemovesHandler()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        Func<CancellationToken, ValueTask> handler = (ct) => ValueTask.CompletedTask;

        weakEvent.Subscribe(handler);

        // Act
        var remainingCount = weakEvent.Unsubscribe(handler);

        // Assert
        Assert.Equal(0, remainingCount);
        Assert.Equal(0, weakEvent.SubscriberCount());
    }

    [Fact]
    public async Task PublishAsync_AfterUnsubscribe_DoesNotExecuteHandler()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        var callCount = 0;
        Action handler = () => callCount++;

        weakEvent.Subscribe(handler);
        weakEvent.Unsubscribe(handler);

        // Act
        await weakEvent.PublishAsync();

        // Assert
        Assert.Equal(0, callCount);
    }

    [Fact]
    public void Unsubscribe_BySubscriber_RemovesAllHandlersForThatSubscriber()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        var subscriber = new EventSubscriber();

        weakEvent.Subscribe(subscriber.Handler1);
        weakEvent.Subscribe(subscriber.Handler2);

        // Act
        var remainingCount = weakEvent.Unsubscribe(subscriber);

        // Assert
        Assert.Equal(0, remainingCount);
        Assert.Equal(0, weakEvent.SubscriberCount());
    }

    [Fact]
    public async Task PublishAsync_WithCollectedSubscriber_RemovesDeadHandler()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        SubscribeWeakHandler(weakEvent);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Act
        await weakEvent.PublishAsync();

        // Assert
        Assert.Equal(0, weakEvent.SubscriberCount());
    }

    [Fact]
    public async Task PublishAsync_WithCollectedSubscriber_DoesNotInvokeHandler()
    {
        // Arrange
        var weakEvent = new WeakEvent();
        var callTracker = new CallTracker();

        SubscribeWeakHandlerWithTracker(weakEvent, callTracker);

        Assert.Equal(1, weakEvent.SubscriberCount());

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Act
        await weakEvent.PublishAsync();

        // Assert
        Assert.Equal(0, callTracker.CallCount); // Handler should not be invoked
        Assert.Equal(0, weakEvent.SubscriberCount()); // Dead handler should be removed
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_CompletesSuccessfully()
    {
        // Arrange
        var weakEvent = new WeakEvent();

        // Act & Assert - should not throw
        await weakEvent.PublishAsync();
    }

    #endregion

    #region WeakEvent<T> (Generic) Tests

    [Fact]
    public void Subscribe_Generic_WithAction_AddsSubscriber()
    {
        // Arrange
        var weakEvent = new WeakEvent<string>();
        Action<string> handler = (s) => { };

        // Act
        weakEvent.Subscribe(handler);

        // Assert
        Assert.Equal(1, weakEvent.SubscriberCount());
    }

    [Fact]
    public void Subscribe_Generic_WithFunc_AddsSubscriber()
    {
        // Arrange
        var weakEvent = new WeakEvent<string>();
        Func<string, ValueTask> handler = (s) => ValueTask.CompletedTask;

        // Act
        weakEvent.Subscribe(handler);

        // Assert
        Assert.Equal(1, weakEvent.SubscriberCount());
    }

    [Fact]
    public void Subscribe_Generic_WithCancellationTokenFunc_AddsSubscriber()
    {
        // Arrange
        var weakEvent = new WeakEvent<string>();
        Func<string, CancellationToken, ValueTask> handler = (s, ct) => ValueTask.CompletedTask;

        // Act
        weakEvent.Subscribe(handler);

        // Assert
        Assert.Equal(1, weakEvent.SubscriberCount());
    }

    [Fact]
    public async Task PublishAsync_Generic_WithAction_ExecutesHandlerWithData()
    {
        // Arrange
        var weakEvent = new WeakEvent<string>();
        string? receivedData = null;
        Action<string> handler = (s) => receivedData = s;

        weakEvent.Subscribe(handler);

        // Act
        await weakEvent.PublishAsync("test-data");

        // Assert
        Assert.Equal("test-data", receivedData);
    }

    [Fact]
    public async Task PublishAsync_Generic_WithMultipleHandlers_ExecutesAllWithSameData()
    {
        // Arrange
        var weakEvent = new WeakEvent<int>();
        var receivedData1 = 0;
        var receivedData2 = 0;
        Action<int> handler1 = (i) => receivedData1 = i;
        Action<int> handler2 = (i) => receivedData2 = i;

        weakEvent.Subscribe(handler1);
        weakEvent.Subscribe(handler2);

        // Act
        await weakEvent.PublishAsync(42);

        // Assert
        Assert.Equal(42, receivedData1);
        Assert.Equal(42, receivedData2);
    }

    [Fact]
    public async Task PublishAsync_Generic_WithAsyncHandler_AwaitsCompletion()
    {
        // Arrange
        var weakEvent = new WeakEvent<string>();
        string? receivedData = null;
        Func<string, ValueTask> handler = async (s) =>
        {
            await Task.Delay(10);
            receivedData = s;
        };

        weakEvent.Subscribe(handler);

        // Act
        await weakEvent.PublishAsync("async-test");

        // Assert
        Assert.Equal("async-test", receivedData);
    }

    [Fact]
    public async Task PublishAsync_Generic_WithCancellationToken_PassesTokenAndData()
    {
        // Arrange
        var weakEvent = new WeakEvent<string>();
        string? receivedData = null;
        CancellationToken receivedToken = default;
        Func<string, CancellationToken, ValueTask> handler = (s, ct) =>
        {
            receivedData = s;
            receivedToken = ct;
            return ValueTask.CompletedTask;
        };

        weakEvent.Subscribe(handler);
        var cts = new CancellationTokenSource();

        // Act
        await weakEvent.PublishAsync("data-with-token", cts.Token);

        // Assert
        Assert.Equal("data-with-token", receivedData);
        Assert.Equal(cts.Token, receivedToken);
    }

    [Fact]
    public void Unsubscribe_Generic_WithAction_RemovesHandler()
    {
        // Arrange
        var weakEvent = new WeakEvent<string>();
        Action<string> handler = (s) => { };

        weakEvent.Subscribe(handler);

        // Act
        var remainingCount = weakEvent.Unsubscribe(handler);

        // Assert
        Assert.Equal(0, remainingCount);
        Assert.Equal(0, weakEvent.SubscriberCount());
    }

    [Fact]
    public void Unsubscribe_Generic_WithFunc_RemovesHandler()
    {
        // Arrange
        var weakEvent = new WeakEvent<string>();
        Func<string, ValueTask> handler = (s) => ValueTask.CompletedTask;

        weakEvent.Subscribe(handler);

        // Act
        var remainingCount = weakEvent.Unsubscribe(handler);

        // Assert
        Assert.Equal(0, remainingCount);
        Assert.Equal(0, weakEvent.SubscriberCount());
    }

    [Fact]
    public void Unsubscribe_Generic_WithCancellationTokenFunc_RemovesHandler()
    {
        // Arrange
        var weakEvent = new WeakEvent<string>();
        Func<string, CancellationToken, ValueTask> handler = (s, ct) => ValueTask.CompletedTask;

        weakEvent.Subscribe(handler);

        // Act
        var remainingCount = weakEvent.Unsubscribe(handler);

        // Assert
        Assert.Equal(0, remainingCount);
        Assert.Equal(0, weakEvent.SubscriberCount());
    }

    [Fact]
    public async Task PublishAsync_Generic_AfterUnsubscribe_DoesNotExecuteHandler()
    {
        // Arrange
        var weakEvent = new WeakEvent<string>();
        var callCount = 0;
        Action<string> handler = (s) => callCount++;

        weakEvent.Subscribe(handler);
        weakEvent.Unsubscribe(handler);

        // Act
        await weakEvent.PublishAsync("test");

        // Assert
        Assert.Equal(0, callCount);
    }

    [Fact]
    public void Unsubscribe_Generic_BySubscriber_RemovesAllHandlersForThatSubscriber()
    {
        // Arrange
        var weakEvent = new WeakEvent<string>();
        var subscriber = new GenericEventSubscriber();

        weakEvent.Subscribe(subscriber.Handler1);
        weakEvent.Subscribe(subscriber.Handler2);

        // Act
        var remainingCount = weakEvent.Unsubscribe(subscriber);

        // Assert
        Assert.Equal(0, remainingCount);
        Assert.Equal(0, weakEvent.SubscriberCount());
    }

    [Fact]
    public async Task PublishAsync_Generic_WithCollectedSubscriber_RemovesDeadHandler()
    {
        // Arrange
        var weakEvent = new WeakEvent<string>();
        SubscribeWeakGenericHandler(weakEvent);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Act
        await weakEvent.PublishAsync("test");

        // Assert
        Assert.Equal(0, weakEvent.SubscriberCount());
    }

    [Fact]
    public async Task PublishAsync_Generic_WithCollectedSubscriber_DoesNotInvokeHandler()
    {
        // Arrange
        var weakEvent = new WeakEvent<string>();
        var callTracker = new CallTracker();
        SubscribeWeakGenericHandlerWithTracker(weakEvent, callTracker);

        Assert.Equal(1, weakEvent.SubscriberCount());

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Act
        await weakEvent.PublishAsync("test-data");

        // Assert
        Assert.Equal(0, callTracker.CallCount); // Handler should not be invoked
        Assert.Equal(0, weakEvent.SubscriberCount()); // Dead handler should be removed
    }

    [Fact]
    public async Task PublishAsync_Generic_WithNoSubscribers_CompletesSuccessfully()
    {
        // Arrange
        var weakEvent = new WeakEvent<string>();

        // Act & Assert - should not throw
        await weakEvent.PublishAsync("test");
    }

    [Fact]
    public async Task PublishAsync_Generic_WithComplexType_PassesCorrectData()
    {
        // Arrange
        var weakEvent = new WeakEvent<TestEventArgs>();
        TestEventArgs? receivedArgs = null;
        Action<TestEventArgs> handler = (args) => receivedArgs = args;

        weakEvent.Subscribe(handler);
        var eventArgs = new TestEventArgs { Message = "test", Value = 123 };

        // Act
        await weakEvent.PublishAsync(eventArgs);

        // Assert
        Assert.NotNull(receivedArgs);
        Assert.Equal("test", receivedArgs.Message);
        Assert.Equal(123, receivedArgs.Value);
    }

    #endregion

    #region Helper Methods and Classes

    private static void SubscribeWeakHandler(WeakEvent weakEvent)
    {
        var subscriber = new EventSubscriber();
        weakEvent.Subscribe(subscriber.Handler1);
    }

    private static void SubscribeWeakHandlerWithTracker(WeakEvent weakEvent, CallTracker tracker)
    {
        var subscriber = new EventSubscriberWithTracker(tracker);
        weakEvent.Subscribe(subscriber.Handler);
    }

    private static void SubscribeWeakGenericHandler(WeakEvent<string> weakEvent)
    {
        var subscriber = new GenericEventSubscriber();
        weakEvent.Subscribe(subscriber.Handler1);
    }

    private static void SubscribeWeakGenericHandlerWithTracker(WeakEvent<string> weakEvent, CallTracker tracker)
    {
        var subscriber = new GenericEventSubscriberWithTracker(tracker);
        weakEvent.Subscribe(subscriber.Handler);
    }

    private class CallTracker
    {
        public int CallCount { get; private set; }

        public void IncrementCallCount() => CallCount++;
    }

    private class EventSubscriber
    {
        public int CallCount1 { get; private set; }
        public int CallCount2 { get; private set; }

        public void Handler1() => CallCount1++;
        public void Handler2() => CallCount2++;
    }

    private class EventSubscriberWithTracker
    {
        private readonly CallTracker _tracker;

        public EventSubscriberWithTracker(CallTracker tracker)
        {
            _tracker = tracker;
        }

        public void Handler() => _tracker.IncrementCallCount();
    }

    private class GenericEventSubscriber
    {
        public int CallCount1 { get; private set; }
        public int CallCount2 { get; private set; }

        public void Handler1(string data) => CallCount1++;
        public void Handler2(string data) => CallCount2++;
    }

    private class GenericEventSubscriberWithTracker
    {
        private readonly CallTracker _tracker;

        public GenericEventSubscriberWithTracker(CallTracker tracker)
        {
            _tracker = tracker;
        }

        public void Handler(string data) => _tracker.IncrementCallCount();
    }

    private class TestEventArgs
    {
        public string Message { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    #endregion
}
