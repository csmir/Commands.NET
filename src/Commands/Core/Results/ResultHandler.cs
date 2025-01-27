using Commands.Conditions;
using Commands.Parsing;

namespace Commands;

/// <summary>
///     A delegate-based handler for post-execution processes.
/// </summary>
/// <remarks>
///     This implementation of <see cref="ResultHandler"/> allows you to define a delegate that will be executed when the command execution is completed. This delegate is only executed if the command failed.
/// </remarks>
/// <typeparam name="TContext">The context type this handler should cover.</typeparam>
/// <param name="resultDelegate">The delegate that will handle the failed execution result.</param>
public sealed class DelegateResultHandler<TContext>(Func<TContext, IExecuteResult, IServiceProvider, ValueTask> resultDelegate)
    : ResultHandler<TContext>
    where TContext : class, ICallerContext
{
    private readonly Func<TContext, IExecuteResult, IServiceProvider, ValueTask> _resultDelegate = resultDelegate;

    /// <inheritdoc />
    public override ValueTask HandleResult(TContext caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (result.Success)
            return HandleSuccess(caller, result, services, cancellationToken);
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
/// <typeparam name="TContext">The context type this handler should cover.</typeparam>
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
    public override ValueTask HandleResult(ICallerContext caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
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
    public virtual ValueTask HandleResult(ICallerContext caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (result is InvokeResult valueResult && valueResult.Success)
            return HandleSuccess(caller, valueResult, services, cancellationToken);

        var message = result.GetMessage()!; // On failure, the exception message is always present.

        switch (result)
        {
            case InvokeResult invoke:
                return HandleInvocationFailed(caller, invoke, message, services, cancellationToken);
            case SearchResult search:
                if (search.Exception is CommandRouteIncompleteException)
                    return HandleRouteIncomplete(caller, search, message, services, cancellationToken);
                return HandleCommandNotFound(caller, search, message, services, cancellationToken);
            case ParseResult parse:
                if (parse.Exception is CommandOutOfRangeException)
                    return HandleCommandOutOfRange(caller, parse, message, services, cancellationToken);
                return HandleConversionFailed(caller, parse, message, services, cancellationToken);
            case ConditionResult condition:
                return HandleConditionUnmet(caller, condition, message, services, cancellationToken);
            default:
                return HandleUnknownResult(caller, result, message, services, cancellationToken);
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
    protected async virtual ValueTask HandleSuccess(ICallerContext caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        async ValueTask Respond(object? obj)
        {
            if (caller is AsyncCallerContext asyncCaller)
                await asyncCaller.Respond(obj).ConfigureAwait(false);
            else
                caller.Respond(obj);
        }

        if (result is not InvokeResult invokeResult)
            return;

        switch (invokeResult.ReturnValue)
        {
            case null: // (void)
                return;

            case Task task:
                await task.ConfigureAwait(false);

                var taskType = task.GetType();

                // If the task is a generic task, and the result is not a void task result, get the result and respond with it. Unfortunately we cannot do a type comparison on VoidTaskResult, because it is an internal corelib struct.
                if (taskType.IsGenericType && taskType.GenericTypeArguments[0].Name != "VoidTaskResult")
                {
                    _taskGetValue ??= taskType.GetProperty("Result")!.GetMethod;

                    var output = _taskGetValue?.Invoke(task, null);

                    if (output != null)
                        await Respond(output).ConfigureAwait(false);
                }
                return;

            case object obj:
                await Respond(obj).ConfigureAwait(false);
                return;
        }
    }

    #region Result Types

    /// <summary>
    ///     Holds the evaluation data of a search operation where a command is not found from the provided match.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="errorReason">The reason why the operation did not succeed.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask HandleCommandNotFound(ICallerContext caller, SearchResult result, string errorReason, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of a search operation where the root of a command is found, but no invokable command is discovered.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="errorReason">The reason why the operation did not succeed.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask HandleRouteIncomplete(ICallerContext caller, SearchResult result, string errorReason, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of a match operation where the argument length of the best match does not match the input query.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="errorReason">The reason why the operation did not succeed.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask HandleCommandOutOfRange(ICallerContext caller, ParseResult result, string errorReason, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of a match operation where one or more arguments did not succeed conversion into target type.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="errorReason">The reason why the operation did not succeed.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask HandleConversionFailed(ICallerContext caller, ParseResult result, string errorReason, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of a check operation where a pre- or postcondition did not succeed evaluation.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="errorReason">The reason why the operation did not succeed.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask HandleConditionUnmet(ICallerContext caller, ConditionResult result, string errorReason, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of an invoke operation where the invocation failed by exception.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="errorReason">The reason why the operation did not succeed.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask HandleInvocationFailed(ICallerContext caller, InvokeResult result, string errorReason, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of an unhandled result.
    /// </summary>
    /// <param name="caller">The caller of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="errorReason">The reason why the operation did not succeed.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask HandleUnknownResult(ICallerContext caller, IExecuteResult result, string errorReason, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    #endregion

    #region Initializers

    /// <inheritdoc cref="From{T}(Action{T, IExecuteResult, IServiceProvider})"/>
    public static ResultHandlerProperties<T> For<T>()
        where T : class, ICallerContext
        => new();

    /// <inheritdoc cref="From{T}(Action{T, IExecuteResult, IServiceProvider})"/>
    public static ResultHandlerProperties<T> From<T>(Func<T, IExecuteResult, IServiceProvider, ValueTask> executionDelegate)
        where T : class, ICallerContext
        => new ResultHandlerProperties<T>().Delegate(executionDelegate);

    /// <summary>
    ///     Defines a collection of properties to configure and convert into a new instance of <see cref="ResultHandler"/>.
    /// </summary>
    /// <param name="executionDelegate">The delegate that should be executed when the handler is invoked.</param>
    /// <returns>A fluent-pattern property object that can be converted into an instance when configured.</returns>
    public static ResultHandlerProperties<T> From<T>(Action<T, IExecuteResult, IServiceProvider> executionDelegate)
        where T : class, ICallerContext
        => new ResultHandlerProperties<T>().Delegate(executionDelegate);

    #endregion
}
