using Commands.Reflection;

namespace Commands
{
    /// <summary>
    ///     The result of an invocation operation within the command execution pipeline.
    /// </summary>
    public readonly struct RunResult : ICommandResult
    {
        /// <inheritdoc />
        public Exception Exception { get; } = null;

        /// <inheritdoc />
        public bool Success { get; }

        /// <summary>
        ///     Gets the command responsible for the invocation.
        /// </summary>
        public CommandInfo Command { get; }

        internal RunResult(CommandInfo command, Exception exception)
        {
            Exception = exception;
            Command = command;
            Success = false;
        }

        internal RunResult(CommandInfo command)
        {
            Command = command;
            Success = true;
        }
    }
}
