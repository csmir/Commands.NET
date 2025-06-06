namespace Commands.Hosting;

internal sealed class ExecutionScope : IExecutionScope
{
    public IContext Context { get; set; } = null!;

    public CancellationTokenSource CancellationSource { get; set; } = null!;

    public IServiceScope Scope { get; set; } = null!;

    public void Populate(IContext context, IServiceScope scope, CancellationTokenSource cancellationSource)
    {
        Scope = scope;
        Context = context;
        CancellationSource = cancellationSource;
    }

    public void Dispose()
    {
        // Dispose of the scope if it was created.
        if (Scope is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (Context is IDisposable contextDisposable)
        {
            contextDisposable.Dispose();
        }

        // Dispose of the cancellation token source.
        CancellationSource?.Dispose();
    }
}
