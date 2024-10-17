using Commands.Reflection;
using System.Diagnostics;

namespace Commands
{
    /// <summary>
    ///     The result of a match operation within the command execution pipeline.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public readonly struct MatchResult : ICommandResult
    {
        /// <summary>
        ///     Gets the command known during the matching operation.
        /// </summary>
        public CommandInfo Command { get; }

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

        internal object?[]? Arguments { get; }

        private MatchResult(CommandInfo command, object[]? arguments, Exception? exception)
        {
            Command = command;
            Arguments = arguments;
            Exception = exception;
        }

        /// <summary>
        ///     Creates a new <see cref="MatchResult"/> resembling a successful match operation.
        /// </summary>
        /// <param name="command">The match command.</param>
        /// <param name="arguments">The converted arguments of the command.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static MatchResult FromSuccess(CommandInfo command, object[] arguments)
        {
            return new(command, arguments, null);
        }

        /// <summary>
        ///     Creates a new <see cref="MatchResult"/> resembling a failed match operation.
        /// </summary>
        /// <param name="command">The match command.</param>
        /// <param name="exception">The exception that occurred during the matching process.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static MatchResult FromError(CommandInfo command, Exception exception)
        {
            return new(command, null, exception);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Command = {Command} \nSuccess = {(Exception == null ? "True" : $"False \nException = {Exception.Message}")}";
        }
    }
}
