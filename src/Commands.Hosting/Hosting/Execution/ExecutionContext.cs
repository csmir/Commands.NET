
using System.Diagnostics.CodeAnalysis;

namespace Commands.Hosting;

internal sealed class ExecutionContext : IExecutionContext
{
    public ICallerContext Caller { get; set; } = null!;

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

    bool IExecutionContext.TryGetCaller<T>([NotNullWhen(true)] out T caller)
    {
        if (Caller is T t)
        {
            caller = t;
            return true;
        }

        caller = default!;
        return false;
    }
}
