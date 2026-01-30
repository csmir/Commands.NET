using Commands.Conditions;

namespace Commands.Http;

/// <summary>
///     An attribute that can be used to evaluate conditions against an HTTP command context.
/// </summary>
/// <typeparam name="T">The evaluation approach for the condition.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public abstract class HttpConditionAttribute<T> : ExecuteConditionAttribute<T>
    where T : IEvaluator, new()
{
    /// <inheritdoc />
    public override ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (context is not HttpCommandContext httpContext)
            return Error($"The provided context is no implementation of {nameof(HttpCommandContext)}.");

        return Evaluate(httpContext, command, services, cancellationToken);
    }

    /// <summary>
    ///     Evaluates the current condition against the provided HTTP command context.
    /// </summary>
    /// <param name="context">The context of the current execution.</param>
    /// <param name="command">The command currently being executed.</param>
    /// <param name="services">The provider used to register modules and inject services.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
    public abstract ValueTask<ConditionResult> Evaluate(HttpCommandContext context, Command command, IServiceProvider services, CancellationToken cancellationToken);

    /// <inheritdoc />
    public override ConditionResult Error(string error)
        => Error(new HttpResult(HttpStatusCode.BadRequest, error));

    /// <summary>
    ///     Creates a <see cref="ConditionResult"/> that represents an error condition with the provided status code and error message.
    /// </summary>
    /// <param name="response">The HTTP response that contains the error details.</param>
    /// <returns>A <see cref="ConditionResult"/> representing the failed evaluation.</returns>
    public virtual ConditionResult Error(HttpResult response)
        => ConditionResult.FromError(new HttpConditionException(response));
}
