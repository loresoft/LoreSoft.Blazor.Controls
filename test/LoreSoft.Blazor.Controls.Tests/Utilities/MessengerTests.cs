using System.Runtime.CompilerServices;

using LoreSoft.Blazor.Controls.Utilities;

namespace LoreSoft.Blazor.Controls.Tests.Utilities;

public class MessengerTests
{
    private record TestMessage(string Content);
    private record AnotherMessage(int Value);

    [Fact]
    public void Subscribe_WithNullSubscriber_ThrowsArgumentNullException()
    {
        // Arrange
        var messenger = new Messenger();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => messenger.Subscribe<TestMessage>(null!, _ => Task.CompletedTask));
    }

    [Fact]
    public void Subscribe_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var messenger = new Messenger();
        var subscriber = new object();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => messenger.Subscribe<TestMessage>(subscriber, null!));
    }

    [Fact]
    public void Subscribe_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var messenger = new Messenger();
        messenger.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => messenger.Subscribe<TestMessage>(new object(), _ => Task.CompletedTask));
    }

    [Fact]
    public async Task PublishAsync_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var messenger = new Messenger();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await messenger.PublishAsync<TestMessage>(null!));
    }

    [Fact]
    public async Task PublishAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var messenger = new Messenger();
        messenger.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await messenger.PublishAsync(new TestMessage("test")));
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_CompletesSuccessfully()
    {
        // Arrange
        var messenger = new Messenger();

        // Act & Assert - Should not throw
        await messenger.PublishAsync(new TestMessage("test"));
    }

    [Fact]
    public async Task Subscribe_AndPublish_InvokesHandler()
    {
        // Arrange
        var messenger = new Messenger();
        var subscriber = new object();
        var receivedMessages = new List<TestMessage>();

        messenger.Subscribe<TestMessage>(subscriber, msg =>
        {
            receivedMessages.Add(msg);
            return Task.CompletedTask;
        });

        // Act
        await messenger.PublishAsync(new TestMessage("Hello"));

        // Assert
        Assert.Single(receivedMessages);
        Assert.Equal("Hello", receivedMessages[0].Content);
    }

    [Fact]
    public async Task Subscribe_MultipleHandlers_AllReceiveMessage()
    {
        // Arrange
        var messenger = new Messenger();
        var subscriber1 = new object();
        var subscriber2 = new object();
        var receivedCount1 = 0;
        var receivedCount2 = 0;

        messenger.Subscribe<TestMessage>(subscriber1, _ =>
        {
            receivedCount1++;
            return Task.CompletedTask;
        });

        messenger.Subscribe<TestMessage>(subscriber2, _ =>
        {
            receivedCount2++;
            return Task.CompletedTask;
        });

        // Act
        await messenger.PublishAsync(new TestMessage("Test"));

        // Assert
        Assert.Equal(1, receivedCount1);
        Assert.Equal(1, receivedCount2);
    }

    [Fact]
    public async Task Subscribe_DifferentMessageTypes_OnlyReceivesCorrectType()
    {
        // Arrange
        var messenger = new Messenger();
        var subscriber = new object();
        var testMessagesReceived = 0;
        var anotherMessagesReceived = 0;

        messenger.Subscribe<TestMessage>(subscriber, _ =>
        {
            testMessagesReceived++;
            return Task.CompletedTask;
        });

        messenger.Subscribe<AnotherMessage>(subscriber, _ =>
        {
            anotherMessagesReceived++;
            return Task.CompletedTask;
        });

        // Act
        await messenger.PublishAsync(new TestMessage("Test"));
        await messenger.PublishAsync(new AnotherMessage(42));

        // Assert
        Assert.Equal(1, testMessagesReceived);
        Assert.Equal(1, anotherMessagesReceived);
    }

    [Fact]
    public async Task Subscribe_ReturnedDisposable_UnsubscribesHandler()
    {
        // Arrange
        var messenger = new Messenger();
        var subscriber = new object();
        var receivedCount = 0;

        var subscription = messenger.Subscribe<TestMessage>(subscriber, _ =>
        {
            receivedCount++;
            return Task.CompletedTask;
        });

        await messenger.PublishAsync(new TestMessage("First"));

        // Act
        subscription.Dispose();
        await messenger.PublishAsync(new TestMessage("Second"));

        // Assert
        Assert.Equal(1, receivedCount);
    }

    [Fact]
    public async Task Subscribe_DisposeTwice_DoesNotThrow()
    {
        // Arrange
        var messenger = new Messenger();
        var subscriber = new object();
        var subscription = messenger.Subscribe<TestMessage>(subscriber, _ => Task.CompletedTask);

        // Act & Assert - Should not throw
        subscription.Dispose();
        subscription.Dispose();
    }

    [Fact]
    public void Unsubscribe_WithNullSubscriber_ThrowsArgumentNullException()
    {
        // Arrange
        var messenger = new Messenger();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => messenger.Unsubscribe(null!));
    }

    [Fact]
    public void Unsubscribe_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var messenger = new Messenger();
        messenger.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => messenger.Unsubscribe(new object()));
    }

    [Fact]
    public async Task Unsubscribe_RemovesAllSubscriptionsForSubscriber()
    {
        // Arrange
        var messenger = new Messenger();
        var subscriber = new object();
        var testMessagesReceived = 0;
        var anotherMessagesReceived = 0;

        messenger.Subscribe<TestMessage>(subscriber, _ =>
        {
            testMessagesReceived++;
            return Task.CompletedTask;
        });

        messenger.Subscribe<AnotherMessage>(subscriber, _ =>
        {
            anotherMessagesReceived++;
            return Task.CompletedTask;
        });

        // Act
        messenger.Unsubscribe(subscriber);
        await messenger.PublishAsync(new TestMessage("Test"));
        await messenger.PublishAsync(new AnotherMessage(42));

        // Assert
        Assert.Equal(0, testMessagesReceived);
        Assert.Equal(0, anotherMessagesReceived);
    }

    [Fact]
    public async Task Unsubscribe_DoesNotAffectOtherSubscribers()
    {
        // Arrange
        var messenger = new Messenger();
        var subscriber1 = new object();
        var subscriber2 = new object();
        var receivedCount1 = 0;
        var receivedCount2 = 0;

        messenger.Subscribe<TestMessage>(subscriber1, _ =>
        {
            receivedCount1++;
            return Task.CompletedTask;
        });

        messenger.Subscribe<TestMessage>(subscriber2, _ =>
        {
            receivedCount2++;
            return Task.CompletedTask;
        });

        // Act
        messenger.Unsubscribe(subscriber1);
        await messenger.PublishAsync(new TestMessage("Test"));

        // Assert
        Assert.Equal(0, receivedCount1);
        Assert.Equal(1, receivedCount2);
    }

    [Fact]
    public async Task Dispose_ClearsAllSubscriptions()
    {
        // Arrange
        var messenger = new Messenger();
        var subscriber = new object();
        var receivedCount = 0;

        messenger.Subscribe<TestMessage>(subscriber, _ =>
        {
            receivedCount++;
            return Task.CompletedTask;
        });

        // Act
        messenger.Dispose();

        // Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await messenger.PublishAsync(new TestMessage("Test")));
        Assert.Equal(0, receivedCount);
    }

    [Fact]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Arrange
        var messenger = new Messenger();

        // Act & Assert - Should not throw
        messenger.Dispose();
        messenger.Dispose();
        messenger.Dispose();
    }

    [Fact]
    public async Task PublishAsync_HandlerThrowsException_OtherHandlersStillExecute()
    {
        // Arrange
        var messenger = new Messenger();
        var subscriber1 = new object();
        var subscriber2 = new object();
        var receivedCount2 = 0;
        var exceptionThrown = false;

        messenger.Subscribe<TestMessage>(subscriber1, _ =>
        {
            exceptionThrown = true;
            throw new InvalidOperationException("Test exception");
        });

        messenger.Subscribe<TestMessage>(subscriber2, _ =>
        {
            receivedCount2++;
            return Task.CompletedTask;
        });

        // Act
        // The PublishAsync awaits all handlers, so if one throws, the whole operation throws
        // This is expected behavior
        var exception = await Record.ExceptionAsync(async () => await messenger.PublishAsync(new TestMessage("Test")));

        // Assert
        // The second handler should still have been called even though the first threw
        Assert.Equal(1, receivedCount2);
        // And an exception should have been captured from the first handler
        Assert.True(exceptionThrown);
        // The PublishAsync itself may throw since it awaits all handlers
        // But we care that both handlers were invoked
    }

    [Fact]
    public async Task PublishAsync_AsyncHandler_AwaitsCompletion()
    {
        // Arrange
        var messenger = new Messenger();
        var subscriber = new object();
        var completed = false;

        messenger.Subscribe<TestMessage>(subscriber, async _ =>
        {
            await Task.Delay(50);
            completed = true;
        });

        // Act
        await messenger.PublishAsync(new TestMessage("Test"));

        // Assert
        Assert.True(completed);
    }

    [Fact]
    public async Task PublishAsync_MultipleAsyncHandlers_AllComplete()
    {
        // Arrange
        var messenger = new Messenger();
        var completionFlags = new bool[3];

        for (int i = 0; i < 3; i++)
        {
            var index = i;
            messenger.Subscribe<TestMessage>(new object(), async _ =>
            {
                await Task.Delay(10);
                completionFlags[index] = true;
            });
        }

        // Act
        await messenger.PublishAsync(new TestMessage("Test"));

        // Assert
        Assert.All(completionFlags, flag => Assert.True(flag));
    }

    [Fact]
    public async Task WeakReference_SubscriberGarbageCollected_SubscriptionNotInvoked()
    {
        // Arrange
        var messenger = new Messenger();
        var receivedCount = 0;

        CreateSubscriptionAndCollect(messenger, () =>
        {
            receivedCount++;
        });

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Give cleanup task time to run
        await Task.Delay(100);

        // Act
        await messenger.PublishAsync(new TestMessage("Test"));

        // Assert
        Assert.Equal(0, receivedCount);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateSubscriptionAndCollect(Messenger messenger, Action onReceived)
    {
        var subscriber = new object();
        messenger.Subscribe<TestMessage>(subscriber, _ =>
        {
            onReceived();
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task Subscribe_SameSubscriberMultipleTimes_AllHandlersInvoked()
    {
        // Arrange
        var messenger = new Messenger();
        var subscriber = new object();
        var receivedCount = 0;

        messenger.Subscribe<TestMessage>(subscriber, _ =>
        {
            receivedCount++;
            return Task.CompletedTask;
        });

        messenger.Subscribe<TestMessage>(subscriber, _ =>
        {
            receivedCount++;
            return Task.CompletedTask;
        });

        // Act
        await messenger.PublishAsync(new TestMessage("Test"));

        // Assert
        Assert.Equal(2, receivedCount);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleMessages_EachHandlerReceivesAll()
    {
        // Arrange
        var messenger = new Messenger();
        var subscriber = new object();
        var receivedMessages = new List<string>();

        messenger.Subscribe<TestMessage>(subscriber, msg =>
        {
            receivedMessages.Add(msg.Content);
            return Task.CompletedTask;
        });

        // Act
        await messenger.PublishAsync(new TestMessage("First"));
        await messenger.PublishAsync(new TestMessage("Second"));
        await messenger.PublishAsync(new TestMessage("Third"));

        // Assert
        Assert.Equal(3, receivedMessages.Count);
        Assert.Equal(new[] { "First", "Second", "Third" }, receivedMessages);
    }
}
