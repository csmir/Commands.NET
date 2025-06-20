namespace Commands.Hosting;

internal sealed class ExecutionScope : IExecutionScope
{
    public IContext Context { get; set; } = null!;

    public IServiceScope Scope { get; set; } = null!;

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
    }
}
