using Commands.Reflection;
using System.Diagnostics;

namespace Commands.Core
{
    /// <summary>
    ///     The result of a match operation within the command execution pipeline.
    /// </summary>
    [DebuggerDisplay("Success = {Success()}")]
    public readonly struct MatchResult : IRunResult
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

        /// <summary>
        ///     Gets the command known during the matching operation.
        /// </summary>
        public CommandInfo Command { get; }

        internal object[] Reads { get; } = null;

        internal MatchResult(CommandInfo command, object[] reads)
        {
            Command = command;
            Reads = reads;
        }

        internal MatchResult(CommandInfo command, Exception exception)
        {
            Command = command;

            Exception = exception;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Command = {Command} \nSuccess = {(Exception == null ? "True" : $"False \nException = {Exception}")}";
        }
    }
}
