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
        public Exception Exception { get; } = null;

        /// <inheritdoc />
        public bool Success
        {
            get
            {
                return Exception == null;
            }
        }

        internal CommandOptions Options { get; } = null;

        internal ConsumerBase Consumer { get; } = null;

        internal object[] Args { get; } = null;

        internal SourceResult(ConsumerBase consumer, object[] args, CommandOptions options)
        {
            Options = options;
            Consumer = consumer;
            Args = args;
        }

        internal SourceResult(ConsumerBase consumer, object[] args)
            : this(consumer, args, null)
        {

        }

        internal SourceResult(Exception exception)
        {
            Exception = exception;
        }

        /// <summary>
        ///     Creates a new successful <see cref="SourceResult"/>.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="args">A parsed command query.</param>
        /// <param name="options">A set of options that determine logic in the command execution.</param>
        /// <returns>A new <see cref="SourceResult"/> from provided arguments.</returns>
        public static SourceResult FromSuccess(ConsumerBase consumer, object[] args, CommandOptions options)
        {
            return new(consumer, args, options);
        }

        /// <summary>
        ///     Creates a new successful <see cref="SourceResult"/>.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="args">A parsed command query.</param>
        /// <returns>A new <see cref="SourceResult"/> from provided arguments.</returns>
        public static SourceResult FromSuccess(ConsumerBase consumer, object[] args)
        {
            return new(consumer, args);
        }

        /// <summary>
        ///     Creates a new unsuccessful <see cref="SourceResult"/>.
        /// </summary>
        /// <param name="exception">An exception describing the failed process.</param>
        /// <returns>A new <see cref="SourceResult"/> from provided arguments.</returns>
        public static SourceResult FromError(Exception exception)
        {
            return new(exception);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Success = {(Exception == null ? "True" : $"False \nException = {Exception}")}";
        }
    }
}
