using Commands.Components;
using System.Diagnostics;

namespace Commands
{
    /// <summary>
    ///     The result of a match operation within the command execution pipeline.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public readonly struct MatchResult : IExecuteResult
    {
        /// <summary>
        ///     Gets the command known during the matching operation.
        /// </summary>
        public CommandInfo Command { get; }

        /// <inheritdoc />
        public Exception? Exception { get; }

        /// <inheritdoc />
        public bool Success
            => Exception == null;

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
            => new(command, arguments, null);

        /// <summary>
        ///     Creates a new <see cref="MatchResult"/> resembling a failed match operation.
        /// </summary>
        /// <param name="command">The match command.</param>
        /// <param name="exception">The exception that occurred during the matching process.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static MatchResult FromError(CommandInfo command, Exception exception)
            => new(command, null, exception);

        /// <inheritdoc />
        public override string ToString()
            => $"Command = {Command} \nSuccess = {(Exception == null ? "True" : $"False \nException = {Exception.Message}")}";

        /// <summary>
        ///     Gets a string representation of this result.
        /// </summary>
        /// <param name="inline">Sets whether the string representation should be inlined or not.</param>
        /// <returns></returns>
        public string ToString(bool inline)
            => inline ? $"Command = {Command} Success = {(Exception == null ? "True" : $"False")}" : ToString();

        /// <summary>
        ///     Implicitly converts a <see cref="MatchResult"/> to a <see cref="ValueTask{TResult}"/>.
        /// </summary>
        /// <param name="result">The result to convert.</param>
        public static implicit operator ValueTask<MatchResult>(MatchResult result)
            => new(result);
    }
}
