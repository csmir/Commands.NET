namespace Commands;

public sealed class ResultHandlerProperties<T> : ResultHandlerProperties
    where T : class, ICallerContext
{
    private Func<T, IExecuteResult, IServiceProvider, ValueTask>? _delegate;

    public ResultHandlerProperties()
        : base(null!) // Assign null as we do not use the underlying logic
    {
        _delegate = null;
    }

    public ResultHandlerProperties<T> Delegate(Action<T, IExecuteResult, IServiceProvider> executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _delegate = (context, result, services) =>
        {
            executionDelegate(context, result, services);
            return default;
        };

        return this;
    }

    public ResultHandlerProperties<T> Delegate(Func<T, IExecuteResult, IServiceProvider, ValueTask> executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _delegate = executionDelegate;

        return this;
    }

    public override ResultHandler ToHandler()
    {
        Assert.NotNull(_delegate, nameof(_delegate));

        return new DelegateResultHandler<T>(_delegate!);
    }
}

public class ResultHandlerProperties
{
    private readonly ResultHandler _handler;

    internal ResultHandlerProperties(ResultHandler handler)
    {
        _handler = handler;
    }

    public virtual ResultHandler ToHandler()
        => _handler;
}