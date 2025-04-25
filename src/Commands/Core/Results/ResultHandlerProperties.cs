namespace Commands;

/// <summary>
///     A set of conditions of a result handler.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ResultHandlerProperties<T> : IResultHandlerProperties
    where T : class, ICallerContext
{
    private Func<T, Exception, IServiceProvider, ValueTask>? _delegate;

    /// <summary>
    ///     Creates a new instance of <see cref="ResultHandlerProperties{T}"/>.
    /// </summary>
    public ResultHandlerProperties()
    {
        _delegate = null;
    }

    /// <summary>
    ///     Sets the delegate that will be executed when the result handler is invoked.
    /// </summary>
    /// <param name="executionDelegate">The delegate to set.</param>
    /// <returns>The same <see cref="ResultHandlerProperties{T}"/> for call-chaining.</returns>
    public ResultHandlerProperties<T> AddDelegate(Action<T, Exception, IServiceProvider> executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _delegate = (context, result, services) =>
        {
            executionDelegate(context, result, services);
            return default;
        };

        return this;
    }

    /// <summary>
    ///     Sets the delegate that will be executed when the result handler is invoked.
    /// </summary>
    /// <param name="executionDelegate">The delegate to set.</param>
    /// <returns>The same <see cref="ResultHandlerProperties{T}"/> for call-chaining.</returns>
    public ResultHandlerProperties<T> AddDelegate(Func<T, Exception, IServiceProvider, ValueTask> executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _delegate = executionDelegate;

        return this;
    }

    /// <inheritdoc />
    public ResultHandler ToHandler()
    {
        Assert.NotNull(_delegate, nameof(_delegate));

        return new DelegateResultHandler<T>(_delegate!);
    }
}

internal readonly struct ResultHandlerProperties : IResultHandlerProperties
{
    private readonly ResultHandler _handler;

    internal ResultHandlerProperties(ResultHandler handler)
    {
        _handler = handler;
    }

    public ResultHandler ToHandler()
        => _handler;
}