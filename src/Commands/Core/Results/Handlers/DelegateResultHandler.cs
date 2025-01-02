namespace Commands;

/// <summary>
///     Represents a resolver that invokes a delegate when a result is encountered from a command implementing <typeparamref name="T"/>. This class cannot be inherited.
/// </summary>
/// <param name="func">The action to be invoked when receiving a result.</param>
public sealed class DelegateResultHandler<T>(
    Func<T, IExecuteResult, IServiceProvider, ValueTask> func)
    : ResultHandler<T>
    where T : class, ICallerContext
{
    /// <inheritdoc />
    public override ValueTask HandleResult(T caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (result.Success && result is IValueResult value)
            return HandleSuccess(caller, value, services, cancellationToken);
        else
            return func(caller, result, services);
    }
}

/// <summary>
///     Represents a resolver that invokes a delegate when a result is encountered. This class cannot be inherited.
/// </summary>
/// <param name="func">The action to be invoked when receiving a result.</param>
public sealed class DelegateResultHandler(
    Func<ICallerContext, IExecuteResult, IServiceProvider, ValueTask> func)
    : ResultHandler
{
    /// <inheritdoc />
    public override async ValueTask HandleResult(
        ICallerContext caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        // If the result is successful and is an invocation result, we can handle it as a successful command.
        if (result.Success && result is IValueResult value)
            await HandleSuccess(caller, value, services, cancellationToken);
        else
            await func(caller, result, services);
    }
}
