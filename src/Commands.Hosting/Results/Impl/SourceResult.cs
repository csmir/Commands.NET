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

        internal ConsumerBase Consumer { get; } = null;

        internal object[] Args { get; } = null;

        internal SourceResult(ConsumerBase consumer, object[] args)
        {
            Consumer = consumer;
            Args = args;
        }

        internal SourceResult(Exception exception)
        {
            Exception = exception;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Success = {(Exception == null ? "True" : $"False \nException = {Exception}")}";
        }
    }
}
