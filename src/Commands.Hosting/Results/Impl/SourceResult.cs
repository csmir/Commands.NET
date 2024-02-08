using Commands.Core;
using System.Diagnostics;

namespace Commands.Results
{
    /// <summary>
    ///     The result of the source acquirement within the command execution pipeline.
    /// </summary>
    [DebuggerDisplay("Success = {Success()}")]
    public readonly struct SourceResult : IRunResult
    {
        /// <inheritdoc />
        public Exception? Exception { get; }

        /// <inheritdoc />
        public bool Success
        {
            get
            {
                return Exception == null;
            }
        }

        internal CommandOptions? Options { get; }

        internal ConsumerBase? Consumer { get; }

        internal object[]? Args { get; }

        private SourceResult(ConsumerBase? consumer, object[]? args, CommandOptions? options, Exception? exception)
        {
            Options = options;
            Consumer = consumer;
            Args = args;
            Exception = exception;
        }

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> resembling a successful sourcing operation.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="args">A parsed command query.</param>
        /// <param name="options">A set of options that determine logic in the command execution.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SourceResult FromSuccess(ConsumerBase consumer, object[] args, CommandOptions options)
        {
            return new(consumer, args, options, null);
        }

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> resembling a successful sourcing operation.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="args">A parsed command query.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SourceResult FromSuccess(ConsumerBase consumer, object[] args)
        {
            return new(consumer, args, null, null);
        }

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> resembling a failed sourcing operation.
        /// </summary>
        /// <param name="exception">An exception describing the failed process.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SourceResult FromError(Exception exception)
        {
            return new(null, null, null, exception);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Success = {(Exception == null ? "True" : $"False \nException = {Exception}")}";
        }
    }
}
