﻿using Commands.Exceptions;
using Commands.Helpers;
using Commands.Results;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Resolvers
{
    /// <summary>
    ///     A handler for pre-execution processes.
    /// </summary>
    public abstract class SourceResolverBase
    {
        internal bool Available { get; set; }

        /// <summary>
        ///     Waits until all hosted services tied to the startup of the application have started.
        /// </summary>
        /// <param name="retryLimit">Escapes this operation after the specified amount of retries.</param>
        /// <param name="timeout">A delay after which this method will try to start listening.</param>
        /// <returns><see langword="true"/> if the application is ready for command inputs. <see langword="false"/> if the application failed to continue before this method escapes.</returns>
        public bool Ready(int retryLimit = 50, int timeout = 50)
        {
            var retryCounter = 0;
            while (!Available)
            {
                if (retryCounter > retryLimit)
                {
                    return false;
                }

                Task.Delay(timeout).Wait();

                retryCounter++;
            }

            return true;
        }

        /// <summary>
        ///     Evaluates pre-execution data, generating consumer data, query data and configuring execution options.
        /// </summary>
        /// <returns>An awaitable <see cref="ValueTask"/> containing the consumer, query and options for the command to be executed.</returns>
        public abstract ValueTask<SourceResult> Evaluate(CancellationToken cancellationToken);

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
                return SourceResult.FromError(convertEx);
            }
            return SourceResult.FromError(SourceException.SourceAcquirementFailed(exception));
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

            return SourceResult.FromError(new SourceException(error));
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
            return SourceResult.FromSuccess(consumer, args);
        }

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> representing a successful evaluation.
        /// </summary>
        /// <param name="consumer">The consumer of the scope created for the execution of this source.</param>
        /// <param name="args">The arguments to be used to run a command.</param>
        /// <param name="options">The options used to configure command execution.</param>
        /// <returns>A <see cref="SourceResult"/> representing the successful evaluation.</returns>
        protected SourceResult Success<T>(T consumer, object[] args, CommandOptions options)
            where T : ConsumerBase
        {
            return SourceResult.FromSuccess(consumer, args, options);
        }
    }
}
