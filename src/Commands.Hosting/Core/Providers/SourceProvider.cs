namespace Commands
{
    /// <summary>
    ///     A provider that waits to receive input from an external source and prompts command invocation with the received terms.
    /// </summary>
    public abstract class SourceProvider
    {
        internal bool ReadAvailable { get; set; }

        /// <summary>
        ///     Waits until all hosted services tied to the startup of the application have started.
        /// </summary>
        /// <param name="retryLimit">Escapes this operation after the specified amount of retries.</param>
        /// <param name="timeout">A delay after which this method will try to start listening.</param>
        /// <returns><see langword="true"/> if the application is ready for command inputs. <see langword="false"/> if the application failed to continue before this method escapes.</returns>
        public bool Ready(int retryLimit = 50, int timeout = 50)
        {
            var retryCounter = 0;
            while (!ReadAvailable)
            {
                if (retryCounter > retryLimit)
                    return false;

                Task.Delay(timeout).Wait();

                retryCounter++;
            }

            return true;
        }

        /// <summary>
        ///     Evaluates pre-execution data, generating consumer data, query data and configuring execution options.
        /// </summary>
        /// <param name="services">The top level service provider, which can be used to retrieve or append available services. Upon returning this operation as success, a scope of the provider will be made for the rest of the execution chain.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> containing the consumer, query and options for the command to be executed.</returns>
        public abstract ValueTask<SourceResult> Receive(IServiceProvider services, CancellationToken cancellationToken);

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> representing a failed evaluation.
        /// </summary>
        /// <param name="exception">The exception that caused the evaluation to fail.</param>
        /// <returns>A <see cref="SourceResult"/> representing the failed evaluation.</returns>
        protected SourceResult Error(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (exception is SourceException convertEx)
                return SourceResult.FromError(convertEx);

            return SourceResult.FromError(SourceException.GetSourceFailed(exception));
        }

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> representing a failed evaluation.
        /// </summary>
        /// <param name="error">The error that caused the evaluation to fail.</param>
        /// <returns>A <see cref="SourceResult"/> representing the failed evaluation.</returns>
        protected SourceResult Error(string error)
        {
            if (string.IsNullOrEmpty(error))
                throw new ArgumentNullException(nameof(error));

            return SourceResult.FromError(new SourceException(error));
        }

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> representing a successful evaluation.
        /// </summary>
        /// <param name="caller">The caller of the scope created for the execution of this source.</param>
        /// <param name="args">The arguments to be used to run a command.</param>
        /// <returns>A <see cref="SourceResult"/> representing the successful evaluation.</returns>
        protected SourceResult Success<T>(T caller, IEnumerable<object> args)
            where T : ICallerContext
            => SourceResult.FromSuccess(caller, args);

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> representing a successful evaluation.
        /// </summary>
        /// <param name="caller">The caller of the scope created for the execution of this source.</param>
        /// <param name="args">The arguments to be used to run a command.</param>
        /// <param name="options">The options used to configure command execution.</param>
        /// <returns>A <see cref="SourceResult"/> representing the successful evaluation.</returns>
        protected SourceResult Success<T>(T caller, IEnumerable<object> args, CommandOptions options)
            where T : ICallerContext
            => SourceResult.FromSuccess(caller, args, options);

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> representing a successful evaluation.
        /// </summary>
        /// <param name="caller">The caller of the scope created for the execution of this source.</param>
        /// <param name="args">The arguments to be used to run a command.</param>
        /// <returns>A <see cref="SourceResult"/> representing the successful evaluation.</returns>
        protected SourceResult Success<T>(T caller, IEnumerable<KeyValuePair<string, object?>> args)
            where T : ICallerContext
            => SourceResult.FromSuccess(caller, args);

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> representing a successful evaluation.
        /// </summary>
        /// <param name="caller">The caller of the scope created for the execution of this source.</param>
        /// <param name="args">The arguments to be used to run a command.</param>
        /// <param name="options">The options used to configure command execution.</param>
        /// <returns>A <see cref="SourceResult"/> representing the successful evaluation.</returns>
        protected SourceResult Success<T>(T caller, IEnumerable<KeyValuePair<string, object?>> args, CommandOptions options)
            where T : ICallerContext
            => SourceResult.FromSuccess(caller, args, options);
    }
}
