namespace Commands;

/// <summary>
///     Represents a resolver that invokes a delegate when a result is encountered from a command implementing <typeparamref name="TCaller"/>. This class cannot be inherited.
/// </summary>
public sealed class DelegateResultHandler<TCaller>
    : ResultHandler<TCaller>
    where TCaller : class, ICallerContext
{
    private readonly Func<TCaller, IExecuteResult, IServiceProvider, ValueTask> _resultDelegate;

    /// <summary>
    ///     Creates a new instance of <see cref="DelegateResultHandler{TCaller}"/> with the specified delegate.
    /// </summary>
    /// <param name="resultDelegate">An awaitable delegate intending to handle a command result.</param>
    public DelegateResultHandler(Func<TCaller, IExecuteResult, IServiceProvider, ValueTask> resultDelegate)
    {
        _resultDelegate = resultDelegate;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="DelegateResultHandler{TCaller}"/> with the specified delegate.
    /// </summary>
    /// <param name="resultDelegate">A void delegate intending to handle a command result.</param>
    public DelegateResultHandler(Action<TCaller, IExecuteResult, IServiceProvider> resultDelegate)
    {
        _resultDelegate = (caller, result, services) =>
        {
            resultDelegate(caller, result, services);
            return default;
        };
    }


    /// <inheritdoc />
    public override ValueTask HandleResult(TCaller caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (result.Success && result is IValueResult value)
            return HandleSuccess(caller, value, services, cancellationToken);
        else
            return _resultDelegate(caller, result, services);
    }
}

/// <summary>
///     Represents a resolver that invokes a delegate when a result is encountered. This class cannot be inherited.
/// </summary>
public sealed class DelegateResultHandler
    : ResultHandler
{
    private readonly Func<ICallerContext, IExecuteResult, IServiceProvider, ValueTask> _resultDelegate;

    /// <summary>
    ///     Creates a new instance of <see cref="DelegateResultHandler"/> with the specified delegate.
    /// </summary>
    /// <param name="resultDelegate">An awaitable delegate intending to handle a command result.</param>
    public DelegateResultHandler(Func<ICallerContext, IExecuteResult, IServiceProvider, ValueTask> resultDelegate)
    {
        _resultDelegate = resultDelegate;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="DelegateResultHandler"/> with the specified delegate.
    /// </summary>
    /// <param name="resultDelegate">A void delegate intending to handle a command result.</param>
    public DelegateResultHandler(Action<ICallerContext, IExecuteResult, IServiceProvider> resultDelegate)
    {
        _resultDelegate = (caller, result, services) =>
        {
            resultDelegate(caller, result, services);
            return default;
        };
    }

    /// <inheritdoc />
    public override async ValueTask HandleResult(
        ICallerContext caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        // If the result is successful and is an invocation result, we can handle it as a successful command.
        if (result.Success && result is IValueResult value)
            await HandleSuccess(caller, value, services, cancellationToken);
        else
            await _resultDelegate(caller, result, services);
    }
}
