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

        /// <summary>
        ///     Gets the command responsible for the invocation.
        /// </summary>
        public CommandInfo Command { get; }

        internal RunResult(CommandInfo command, Exception exception)
        {
            Exception = exception;
            Command = command;
        }

        internal RunResult(CommandInfo command)
        {
            Command = command;
        }

        /// <inheritdoc />
        public bool Success()
        {
            return Exception == null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Success = {(Exception != null ? "True" : $"False: {Exception}")}";
        }
    }
}
