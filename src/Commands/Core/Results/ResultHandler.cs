namespace Commands;

/// <summary>
///     Represents a resolver that invokes a delegate when a result is encountered from a command implementing <typeparamref name="TContext"/>. This class cannot be inherited.
/// </summary>
public sealed class DelegateResultHandler<TContext>
    : ResultHandler<TContext>
    where TContext : class, ICallerContext
{
    private readonly Func<TContext, IExecuteResult, IServiceProvider, ValueTask> _resultDelegate;

    /// <summary>
    ///     Creates a new instance of <see cref="DelegateResultHandler{TCaller}"/> with the specified delegate.
    /// </summary>
    /// <param name="resultDelegate">An awaitable delegate intending to handle a command result.</param>
    public DelegateResultHandler(Func<TContext, IExecuteResult, IServiceProvider, ValueTask> resultDelegate)
    {
        _resultDelegate = resultDelegate;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="DelegateResultHandler{TCaller}"/> with the specified delegate.
    /// </summary>
    /// <param name="resultDelegate">A void delegate intending to handle a command result.</param>
    public DelegateResultHandler(Action<TContext, IExecuteResult, IServiceProvider> resultDelegate)
    {
        _resultDelegate = (caller, result, services) =>
        {
            resultDelegate(caller, result, services);
            return default;
        };
    }

    /// <inheritdoc />
    public override ValueTask HandleResult(TContext caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (result.Success && result is IValueResult value)
            return HandleSuccess(caller, value, services, cancellationToken);
        else
            return _resultDelegate(caller, result, services);
    }
}

/// <summary>
///     A handler for post-execution processes bound to specific types of <see cref="ICallerContext"/>. This generic handler filters results based on the caller type.
/// </summary>
/// <remarks>
///     Implementing this type allows you to treat result data and scope finalization of all commands executed by the provided <see cref="ICallerContext"/>, regardless on whether the command execution succeeded or not.
/// </remarks>
/// <typeparam name="TContext"></typeparam>
public abstract class ResultHandler<TContext> : ResultHandler
    where TContext : class, ICallerContext
{
    /// <inheritdoc cref="ResultHandler.HandleResult(ICallerContext, IExecuteResult, IServiceProvider, CancellationToken)"/>.
    /// <remarks>
    ///     This method is only executed when the provided <paramref name="caller"/> is of type <typeparamref name="TContext"/>.
    /// </remarks>
    public virtual ValueTask HandleResult(TContext caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
        => base.HandleResult(caller, result, services, cancellationToken);

    /// <inheritdoc />
    public override ValueTask HandleResult(
        ICallerContext caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (caller is TContext typedCaller)
            return HandleResult(typedCaller, result, services, cancellationToken);

        // If the caller is not of type T, return default, not handling the result.
        return default;
    }
}

/// <summary>
///     A handler for post-execution processes.
/// </summary>
/// <remarks>
///     Implementing this type allows you to treat result data and scope finalization, regardless on whether the command execution succeeded or not.
/// </remarks>
public abstract class ResultHandler
{
    private static MethodInfo? _taskGetValue;

    /// <summary>
    ///     Evaluates post-execution data, carrying result, caller data and the scoped <see cref="IServiceProvider"/> for the current execution.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    public virtual ValueTask HandleResult(
        ICallerContext caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (result is IValueResult valueResult && valueResult.Success)
            return HandleSuccess(caller, valueResult, services, cancellationToken);

        switch (result)
        {
            case InvokeResult invoke:
                return HandleInvocationFailed(caller, invoke, services, cancellationToken);
            case SearchResult search:
                if (search.Component != null)
                    return HandleSearchIncomplete(caller, search, services, cancellationToken);
                return HandleCommandNotFound(caller, search, services, cancellationToken);
            case MatchResult match:
                if (match.Arguments != null)
                    return HandleConversionFailed(caller, match, services, cancellationToken);
                return HandleArgumentMismatch(caller, match, services, cancellationToken);
            case ConditionResult condition:
                return HandleConditionUnmet(caller, condition, services, cancellationToken);
            default:
                return HandleUnknownResult(caller, result, services, cancellationToken);
        }
    }

    /// <summary>
    ///     Holds the evaluation data of a successful command execution.
    /// </summary>
    /// <remarks>
    ///     Implement this method to handle the result of a successful command execution. By default, this method will respond to the <paramref name="caller"/> with the result of the command execution.
    /// </remarks>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
#if NET8_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Task<>))]
    [UnconditionalSuppressMessage("AotAnalysis", "IL2075", Justification = "The availability of Task<> is ensured at compile-time.")]
#endif
    protected async virtual ValueTask HandleSuccess(
        ICallerContext caller, IValueResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        async ValueTask Respond(object? obj)
        {
            if (caller is AsyncCallerContext asyncCaller)
                await asyncCaller.Respond(obj);
            else
                caller.Respond(obj);
        }

        if (result is InvokeResult invokeResult)
        {
            switch (invokeResult.Value)
            {
                case null: // (void)
                    return;

                case Task task:
                    await task;

                    var taskType = task.GetType();

                    // If the task is a generic task, and the result is not a void task result, get the result and respond with it. Unfortunately we cannot do a type comparison on VoidTaskResult, because it is an internal corelib struct.
                    if (taskType.IsGenericType && taskType.GenericTypeArguments[0].Name != "VoidTaskResult")
                    {
                        _taskGetValue ??= taskType.GetProperty("Result")!.GetMethod;

                        var output = _taskGetValue?.Invoke(task, null);

                        if (output != null)
                            await Respond(output);
                    }
                    return;

                case object obj:
                    await Respond(obj);
                    return;
            }
        }
    }

    /// <summary>
    ///     Holds the evaluation data of a search operation where a command is not found from the provided match.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask HandleCommandNotFound(
        ICallerContext caller, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of a search operation where the root of a command is found, but no invokable command is discovered.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask HandleSearchIncomplete(
        ICallerContext caller, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of a match operation where the argument length of the best match does not match the input query.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask HandleArgumentMismatch(
        ICallerContext caller, MatchResult result, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of a match operation where one or more arguments did not succeed conversion into target type.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask HandleConversionFailed(
        ICallerContext caller, MatchResult result, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of a check operation where a pre- or postcondition did not succeed evaluation.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask HandleConditionUnmet(
        ICallerContext caller, ConditionResult result, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of an invoke operation where the invocation failed by exception.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask HandleInvocationFailed(
        ICallerContext caller, InvokeResult result, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of an unhandled result.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask HandleUnknownResult(
        ICallerContext caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
        => default;
}
