namespace Commands.Hosting;

internal sealed class ContextAccessor<TContext>(IExecutionScope context) : IContextAccessor<TContext>
    where TContext : IContext
{
    public TContext Context
    {
        get
        {
            if (context.TryGetContext<TContext>(out var typedContext))
                return typedContext;

            throw new InvalidOperationException($"The context of type {typeof(TContext)} is not available in the current scope.");
        }
    }
}
