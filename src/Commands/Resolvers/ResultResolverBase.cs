using Commands.Reflection;
using System.Reflection;

namespace Commands.Resolvers
{
    /// <summary>
    ///     A handler for post-execution processes.
    /// </summary>
    /// <remarks>
    ///     Implementing this type allows you to treat result data and scope finalization, regardless on whether the command execution succeeded or not.
    /// </remarks>
    public abstract class ResultResolverBase
    {
        private readonly static Func<object, object>[] _taskResultPropertyCallers = new Func<object, object>[2];

        /// <summary>
        ///     Evaluates the post-execution data, carrying result data, consumer data and the scoped <see cref="IServiceProvider"/> for the current execution.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        public virtual ValueTask Evaluate(
            ConsumerBase consumer, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (result.Success)
                return default;

            switch (result)
            {
                case SearchResult search:
                    if (search.Component != null)
                        return SearchIncomplete(consumer, search, services, cancellationToken);
                    return CommandNotFound(consumer, search, services, cancellationToken);

                case MatchResult match:
                    if (match.Arguments != null)
                        return ConversionFailed(consumer, match, services, cancellationToken);
                    return ArgumentMismatch(consumer, match, services, cancellationToken);

                case ConditionResult condition:
                    return ConditionUnmet(consumer, condition, services, cancellationToken);

                case InvokeResult invoke:
                    return InvocationFailed(consumer, invoke, services, cancellationToken);

                default:
                    return UnhandledFailure(consumer, result, services, cancellationToken);
            }
        }

        /// <summary>
        ///     Handles the return type of the command, sending the result to the consumer if the return type is not a non-generic task type or void.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="command">The current command which returned a return value.</param>
        /// <param name="value">The value returned by the command.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        public virtual async ValueTask Respond(
            ConsumerBase consumer, CommandInfo command, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            switch (value)
            {
                case null: // (void)
                    break;

                case Task awaitablet:
                    await awaitablet;

                    var ttype = command.Invoker.GetReturnType()!;

                    if (ttype.IsGenericType)
                    {
                        _taskResultPropertyCallers[0] ??= ttype.GetProperty("Result").GetValue;

                        var result = _taskResultPropertyCallers[0](awaitablet);

                        if (result != null)
                            await consumer.Send(result);
                    }
                    break;

                case ValueTask awaitablevt:
                    await awaitablevt;

                    var vttype = command.Invoker.GetReturnType()!;

                    if (vttype.IsGenericType)
                    {
                        _taskResultPropertyCallers[1] ??= vttype.GetProperty("Result").GetValue;

                        var result = _taskResultPropertyCallers[1](awaitablevt);

                        if (result != null)
                            await consumer.Send(result);
                    }
                    break;

                case object obj:
                    if (obj != null)
                        await consumer.Send(obj);
                    break;
            }

            return;
        }

        /// <summary>
        ///     Holds the evaluation data of a search operation where a command is not found from the provided match.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        protected virtual ValueTask CommandNotFound(
            ConsumerBase consumer, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return default;
        }

        /// <summary>
        ///     Holds the evaluation data of a search operation where the root of a command is found, but no invokable command is discovered.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        protected virtual ValueTask SearchIncomplete(
            ConsumerBase consumer, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return default;
        }

        /// <summary>
        ///     Holds the evaluation data of a match operation where the argument length of the best match does not match the input query.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        protected virtual ValueTask ArgumentMismatch(
            ConsumerBase consumer, MatchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return default;
        }

        /// <summary>
        ///     Holds the evaluation data of a match operation where one or more arguments did not succeed conversion into target type.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        protected virtual ValueTask ConversionFailed(
            ConsumerBase consumer, MatchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return default;
        }

        /// <summary>
        ///     Holds the evaluation data of a check operation where a pre- or postcondition did not succeed evaluation.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        protected virtual ValueTask ConditionUnmet(
            ConsumerBase consumer, ConditionResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return default;
        }

        /// <summary>
        ///     Holds the evaluation data of an invoke operation where the invocation failed by exception.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        protected virtual ValueTask InvocationFailed(
            ConsumerBase consumer, InvokeResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return default;
        }

        /// <summary>
        ///     Holds the evaluation data of an unhandled result.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        protected virtual ValueTask UnhandledFailure(
            ConsumerBase consumer, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return default;
        }
    }
}
