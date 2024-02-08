using Commands.Core;
using Commands.Exceptions;
using Commands.Helpers;
using Commands.Results;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Resolvers
{
    public abstract class SourceResolverBase
    {
        const string _exHeader = "Postcondition evaluation failed. View inner exception for more details.";

        public abstract ValueTask<SourceResult> EvaluateAsync();

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> representing a failed evaluation.
        /// </summary>
        /// <param name="exception">The exception that caused the evaluation to fail.</param>
        /// <returns>A <see cref="SourceResult"/> representing the failed evaluation.</returns>
        protected SourceResult Error([DisallowNull] Exception exception)
        {
            if (exception == null)
            {
                ThrowHelpers.ThrowInvalidArgument(exception);
            }

            if (exception is SourceException convertEx)
            {
                return new(convertEx);
            }
            return new(new SourceException(_exHeader, exception));
        }

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> representing a failed evaluation.
        /// </summary>
        /// <param name="error">The error that caused the evaluation to fail.</param>
        /// <returns>A <see cref="SourceResult"/> representing the failed evaluation.</returns>
        protected SourceResult Error([DisallowNull] string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                ThrowHelpers.ThrowInvalidArgument(error);
            }

            return new(new SourceException(error));
        }

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> representing a successful evaluation.
        /// </summary>
        /// <param name="consumer">The consumer of the scope created for the execution of this source.</param>
        /// <param name="args">The arguments to be used to run a command.</param>
        /// <returns>A <see cref="SourceResult"/> representing the successful evaluation.</returns>
        protected SourceResult Success<T>(T consumer, params object[] args)
            where T : ConsumerBase
        {
            return new(consumer, args);
        }
    }
}
