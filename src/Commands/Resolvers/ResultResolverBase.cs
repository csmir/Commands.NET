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
            if (!result.Success)
            {
                switch (result)
                {
                    case SearchResult search:
                        {
                            if (search.Component != null)
                            {
                                return SearchIncomplete(consumer, search, services, cancellationToken);
                            }
                            return CommandNotFound(consumer, search, services, cancellationToken);
                        }
                    case MatchResult match:
                        {
                            if (match.Arguments != null)
                            {
                                return ConversionFailed(consumer, match, services, cancellationToken);
                            }
                            return ArgumentMismatch(consumer, match, services, cancellationToken);
                        }
                    case ConditionResult condition:
                        {
                            return ConditionUnmet(consumer, condition, services, cancellationToken);
                        }
                    case InvokeResult invoke:
                        {
                            return InvocationFailed(consumer, invoke, services, cancellationToken);
                        }
                    default:
                        {
                            return UnhandledFailure(consumer, result, services, cancellationToken);
                        }
                }
            }

            return default;
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
