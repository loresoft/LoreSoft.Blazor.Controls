using LoreSoft.Blazor.Controls.Events;

namespace LoreSoft.Blazor.Controls.Tests.Events;

public class WeakDelegateTests
{
    private int _callCount;
    private string? _lastParameter;

    [Fact]
    public void IsAlive_WithStrongReference_ReturnsTrue()
    {
        // Arrange
        Action handler = () => { };
        var weakDelegate = new WeakDelegate(handler);

        // Act & Assert
        Assert.True(weakDelegate.IsAlive);
    }

    [Fact]
    public void IsAlive_WithInstanceMethod_WhileTargetAlive_ReturnsTrue()
    {
        // Arrange
        var target = new TestTarget();
        Action handler = target.HandleEvent;

        var weakDelegate = new WeakDelegate(handler);

        // Act & Assert
        Assert.True(weakDelegate.IsAlive);
    }

    [Fact]
    public void IsAlive_WithInstanceMethod_AfterTargetCollected_ReturnsFalse()
    {
        // Arrange
        WeakDelegate weakDelegate = CreateWeakDelegateWithCollectibleTarget();

        // Act
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Assert
        Assert.False(weakDelegate.IsAlive);
    }

    [Fact]
    public async Task InvokeAsync_WithSyncAction_ExecutesSuccessfully()
    {
        // Arrange
        _callCount = 0;
        Action handler = () => _callCount++;

        var weakDelegate = new WeakDelegate(handler);

        // Act
        await weakDelegate.InvokeAsync();

        // Assert
        Assert.Equal(1, _callCount);
    }

    [Fact]
    public async Task InvokeAsync_WithSyncActionWithParameter_ExecutesSuccessfully()
    {
        // Arrange
        _lastParameter = null;
        Action<string> handler = (s) => _lastParameter = s;

        var weakDelegate = new WeakDelegate(handler);

        // Act
        await weakDelegate.InvokeAsync("test");

        // Assert
        Assert.Equal("test", _lastParameter);
    }

    [Fact]
    public async Task InvokeAsync_WithTaskReturningHandler_AwaitsCompletion()
    {
        // Arrange
        _callCount = 0;
        Func<Task> handler = async () =>
        {
            await Task.Delay(10);
            _callCount++;
        };

        var weakDelegate = new WeakDelegate(handler);

        // Act
        await weakDelegate.InvokeAsync();

        // Assert
        Assert.Equal(1, _callCount);
    }

    [Fact]
    public async Task InvokeAsync_WithValueTaskReturningHandler_AwaitsCompletion()
    {
        // Arrange
        _callCount = 0;
        Func<ValueTask> handler = async () =>
        {
            await Task.Delay(10);
            _callCount++;
        };

        var weakDelegate = new WeakDelegate(handler);

        // Act
        await weakDelegate.InvokeAsync();

        // Assert
        Assert.Equal(1, _callCount);
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationSupport_PassesCancellationToken()
    {
        // Arrange
        CancellationToken receivedToken = default;
        Func<CancellationToken, Task> handler = (ct) =>
        {
            receivedToken = ct;
            return Task.CompletedTask;
        };

        var weakDelegate = new WeakDelegate(handler);
        var cts = new CancellationTokenSource();

        // Act
        await weakDelegate.InvokeAsync(cancellationToken: cts.Token);

        // Assert
        Assert.Equal(cts.Token, receivedToken);
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationSupportAndParameters_AppendsToken()
    {
        // Arrange
        string? receivedString = null;
        CancellationToken receivedToken = default;
        Func<string, CancellationToken, Task> handler = (s, ct) =>
        {
            receivedString = s;
            receivedToken = ct;
            return Task.CompletedTask;
        };

        var weakDelegate = new WeakDelegate(handler);
        var cts = new CancellationTokenSource();

        // Act
        await weakDelegate.InvokeAsync("test", cts.Token);

        // Assert
        Assert.Equal("test", receivedString);
        Assert.Equal(cts.Token, receivedToken);
    }

    [Fact]
    public async Task InvokeAsync_WithoutCancellationSupport_DoesNotPassToken()
    {
        // Arrange
        _lastParameter = null;
        Action<string> handler = (s) => _lastParameter = s;

        var weakDelegate = new WeakDelegate(handler);
        var cts = new CancellationTokenSource();

        // Act
        await weakDelegate.InvokeAsync("test", cts.Token);

        // Assert
        Assert.Equal("test", _lastParameter);
    }

    [Fact]
    public async Task InvokeAsync_WhenTargetCollected_ReturnsCompletedTask()
    {
        // Arrange
        WeakDelegate weakDelegate = CreateWeakDelegateWithCollectibleTarget();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Act
        await weakDelegate.InvokeAsync();

        // Assert - should not throw and return completed task
        Assert.True(weakDelegate.IsAlive == false);
    }

    [Fact]
    public void IsDelegateMatch_WithSameDelegate_ReturnsTrue()
    {
        // Arrange
        var target = new TestTarget();
        Action handler = target.HandleEvent;

        var weakDelegate = new WeakDelegate(handler);

        // Act & Assert
        Assert.True(weakDelegate.IsDelegateMatch(handler));
    }

    [Fact]
    public void IsDelegateMatch_WithDifferentDelegate_ReturnsFalse()
    {
        // Arrange
        var target1 = new TestTarget();
        var target2 = new TestTarget();

        Action handler1 = target1.HandleEvent;
        Action handler2 = target2.HandleEvent;

        var weakDelegate = new WeakDelegate(handler1);

        // Act & Assert
        Assert.False(weakDelegate.IsDelegateMatch(handler2));
    }

    [Fact]
    public void IsDelegateMatch_WithSameStaticMethod_ReturnsTrue()
    {
        // Arrange
        Action handler = StaticHandler;

        var weakDelegate = new WeakDelegate(handler);

        // Act & Assert
        Assert.True(weakDelegate.IsDelegateMatch(handler));
    }

    [Fact]
    public void IsTargetMatch_WithSameTarget_ReturnsTrue()
    {
        // Arrange
        var target = new TestTarget();
        Action handler = target.HandleEvent;

        var weakDelegate = new WeakDelegate(handler);

        // Act & Assert
        Assert.True(weakDelegate.IsTargetMatch(target));
    }

    [Fact]
    public void IsTargetMatch_WithDifferentTarget_ReturnsFalse()
    {
        // Arrange
        var target1 = new TestTarget();
        var target2 = new TestTarget();

        Action handler = target1.HandleEvent;

        var weakDelegate = new WeakDelegate(handler);

        // Act & Assert
        Assert.False(weakDelegate.IsTargetMatch(target2));
    }

    [Fact]
    public async Task InvokeAsync_WithInstanceMethod_ExecutesOnCorrectTarget()
    {
        // Arrange
        var target = new TestTarget();

        Action handler = target.HandleEvent;

        var weakDelegate = new WeakDelegate(handler);

        // Act
        await weakDelegate.InvokeAsync();

        // Assert
        Assert.Equal(1, target.CallCount);
    }

    private static WeakDelegate CreateWeakDelegateWithCollectibleTarget()
    {
        var target = new TestTarget();

        Action handler = target.HandleEvent;

        return new WeakDelegate(handler);
    }

    private static void StaticHandler()
    {
        // Static handler for testing
    }

    private class TestTarget
    {
        public int CallCount { get; private set; }

        public void HandleEvent()
        {
            CallCount++;
        }
    }
}
