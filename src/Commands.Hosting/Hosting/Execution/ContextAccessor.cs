namespace Commands.Hosting;

internal sealed class ContextAccessor<TContext>(IExecutionContext context) : IContextAccessor<TContext>
    where TContext : ICallerContext
{
    public TContext Context
    {
        get
        {
            if (context.TryGetCaller<TContext>(out var caller))
                return caller;

            throw new InvalidOperationException($"The caller of type {typeof(TContext)} is not available in the current context.");
        }
    }
}
