using Commands.Conditions;
using Commands.Parsing;

namespace Commands.Hosting;

/// <summary>
///     Represents a handler for command execution results, allowing for custom handling of different result types and exceptions.
/// </summary>
public abstract class ResultHandler
{
    /// <summary>
    ///     Handles the result of a command execution, allowing for custom handling of different result types and exceptions.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="exception">The exception that occurred during execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    public virtual ValueTask Failure(IContext context, IResult result, Exception exception, IServiceProvider services, CancellationToken cancellationToken = default)
    {
        try
        {
            switch (result)
            {
                case SearchResult searchResult:
                    {
                        if (exception is CommandRouteIncompleteException routeEx)
                            return RouteIncomplete(context, routeEx, searchResult, services, cancellationToken);

                        if (exception is CommandNotFoundException foundEx)
                            return CommandNotFound(context, foundEx, searchResult, services, cancellationToken);
                    }
                    break;
                case ParseResult parseResult:
                    {
                        if (exception is ParserException parseEx)
                            return ParseFailed(context, parseEx, parseResult, services, cancellationToken);

                        if (exception is CommandOutOfRangeException rangeEx)
                            return ParamsOutOfRange(context, rangeEx, parseResult, services, cancellationToken);
                    }
                    break;
                case ConditionResult conditionResult:
                    {
                        if (exception is ConditionException conditionEx)
                            return ConditionUnmet(context, conditionEx, conditionResult, services, cancellationToken);
                    }
                    break;
                case InvokeResult invokeResult:
                        return InvokeFailed(context, exception, invokeResult, services, cancellationToken);
            }

            return Unhandled(context, exception, result, services, cancellationToken);

        }
        catch (Exception ex)
        {
            return Unhandled(context, ex, result, services, cancellationToken);
        }
    }

    /// <summary>
    ///     Handles the successful result of a command execution, allowing for custom handling of the result and services used in the execution.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    public virtual ValueTask Success(IContext context, IResult result, IServiceProvider services, CancellationToken cancellationToken = default)
        => default;

    /// <summary>
    ///     Holds the evaluation data of a search operation where a command is not found from the provided match.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="exception">The exception that occurred during execution.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask CommandNotFound(IContext context, CommandNotFoundException exception, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of a search operation where the root of a command is found, but no invokable command is discovered.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="exception">The exception that occurred during execution.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask RouteIncomplete(IContext context, CommandRouteIncompleteException exception, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of a match operation where the argument length of the best match does not match the input query.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="exception">The exception that occurred during execution.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask ParamsOutOfRange(IContext context, CommandOutOfRangeException exception, ParseResult result, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of a match operation where one or more arguments did not succeed conversion into target type.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="exception">The exception that occurred during execution.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask ParseFailed(IContext context, ParserException exception, ParseResult result, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of a check operation where a pre- or postcondition did not succeed evaluation.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="exception">The exception that occurred during execution.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask ConditionUnmet(IContext context, ConditionException exception, ConditionResult result, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of an invoke operation where the invocation failed by exception.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="exception">The exception that occurred during execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask InvokeFailed(IContext context, Exception exception, InvokeResult result, IServiceProvider services, CancellationToken cancellationToken)
        => default;

    /// <summary>
    ///     Holds the evaluation data of an unhandled result.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="exception">The exception that occurred during execution.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    protected virtual ValueTask Unhandled(IContext context, Exception? exception, IResult result, IServiceProvider services, CancellationToken cancellationToken)
        => default;
}
