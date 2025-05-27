namespace Commands.Hosting;

internal sealed class ExecutionScope : IExecutionScope
{
    public IContext Context { get; set; } = null!;

    public CancellationTokenSource CancellationSource { get; set; } = null!;

    public IServiceScope Scope { get; set; } = null!;

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
