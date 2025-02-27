namespace LoreSoft.Blazor.Controls.Utilities;

public readonly struct DisposableBag() : IDisposable
{
    private readonly Stack<IDisposable> _disposeStack = new();

    public T Create<T>() where T : IDisposable, new()
    {
        var disposable = new T();
        _disposeStack.Push(disposable);

        return disposable;
    }

    public T Create<T>(Func<T> creator) where T : IDisposable
    {
        var disposable = creator();
        _disposeStack.Push(disposable);

        return disposable;
    }

    public T Include<T>(T disposable) where T : IDisposable
    {
        _disposeStack.Push(disposable);

        return disposable;
    }

    public void Dispose()
    {
        while (_disposeStack.Count > 0)
        {
            var disposable = _disposeStack.Pop();
            disposable.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
