namespace Commands.Hosting;

internal sealed class ContextAccessor<TContext>(IExecutionScope scope) : IContextAccessor<TContext>
    where TContext : IContext
{
    private TContext? _context;

    public TContext Context
    {
        get
        {
            _context ??= scope.Context is TContext ctx
                ? ctx
                : throw new InvalidCastException($"The context of type {typeof(TContext)} is not available in the current scope, being an implementation of {scope.Context.GetType()}");

            return _context;
        }
    }
}
