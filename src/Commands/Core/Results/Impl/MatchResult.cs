using Commands.Reflection;

namespace Commands
{
    /// <summary>
    ///     The result of a match operation within the command execution pipeline.
    /// </summary>
    public readonly struct MatchResult : ICommandResult
    {
        /// <inheritdoc />
        public Exception Exception { get; } = null;

        /// <inheritdoc />
        public bool Success { get; }

        /// <summary>
        ///     Gets the command known during the matching operation.
        /// </summary>
        public CommandInfo Command { get; }

        internal object[] Reads { get; } = null;

        internal MatchResult(CommandInfo command, object[] reads)
        {
            Command = command;
            Reads = reads;
            Success = true;
        }

        internal MatchResult(CommandInfo command, Exception exception)
        {
            Command = command;
            Success = false;

            Exception = exception;
        }
    }
}
