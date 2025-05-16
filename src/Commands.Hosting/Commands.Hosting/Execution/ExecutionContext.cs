namespace Commands.Hosting;

internal sealed class ExecutionContext : IExecutionScope
{
    public IContext Context { get; set; } = null!;

    public CancellationTokenSource CancellationSource { get; set; } = null!;

    public IServiceScope Scope { get; set; } = null!;

    public bool TryGetContext<T>([NotNullWhen(true)] out T context)
        where T : IContext
    {
        if (Context is T t)
        {
            context = t;
            return true;
        }

        context = default!;
        return false;
    }

    public void Dispose()
    {
        // Dispose of the scope if it was created.
        if (Scope is IDisposable disposable)
        {
            disposable.Dispose();
        }

        // Dispose of the cancellation token source.
        CancellationSource?.Dispose();
    }
}
