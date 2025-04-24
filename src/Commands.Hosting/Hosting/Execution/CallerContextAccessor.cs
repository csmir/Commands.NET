namespace Commands.Hosting;

internal sealed class CallerContextAccessor<TCaller>(IExecutionContext context) : ICallerContextAccessor<TCaller>
    where TCaller : ICallerContext
{
    public TCaller Caller
    {
        get
        {
            if (context.TryGetCaller<TCaller>(out var caller))
                return caller;
            
            throw new InvalidOperationException($"The caller of type {typeof(TCaller)} is not available in the current context.");
        }
    }
}
