namespace Commands.Hosting.Hosting.Execution;

internal sealed class CallerContextAccessor<T>(IExecutionContext context) : ICallerContextAccessor<T>
    where T : class, ICallerContext
{
    public T Caller
    {
        get
        {
            if (context.TryGetCaller<T>(out var caller))
                return caller;
            else
                throw new InvalidOperationException($"The caller of type {typeof(T)} is not available in the current context.");
        }
    }
}
