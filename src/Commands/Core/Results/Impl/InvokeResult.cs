using Commands.Reflection;
using System.Diagnostics;

namespace Commands
{
    /// <summary>
    ///     The result of an invocation operation within the command execution pipeline.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public readonly struct InvokeResult : IExecuteResult
    {
        /// <summary>
        ///     Gets the command responsible for the invocation.
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

        private InvokeResult(CommandInfo command, Exception? exception)
        {
            Command = command;
            Exception = exception;
        }

        /// <summary>
        ///     Creates a new <see cref="InvokeResult"/> resembling a successful invocation operation.
        /// </summary>
        /// <param name="command">The command that was invoked.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static InvokeResult FromSuccess(CommandInfo command)
        {
            return new(command, null);
        }

        /// <summary>
        ///     Creates a new <see cref="InvokeResult"/> resembling a failed invocation operation.
        /// </summary>
        /// <param name="command">The command that failed to be invoked</param>
        /// <param name="exception">The exception that occurred during command invocation.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static InvokeResult FromError(CommandInfo command, Exception exception)
        {
            return new(command, exception);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Command = {Command} \nSuccess = {(Exception == null ? "True" : $"False \nException = {Exception.Message}")}";
        }

        /// <summary>
        ///     Gets a string representation of this result.
        /// </summary>
        /// <param name="inline">Sets whether the string representation should be inlined or not.</param>
        /// <returns></returns>
        public string ToString(bool inline)
        {
            if (inline)
            {
                return $"Success = {(Exception == null ? "True" : $"False")}";
            }
            else
            {
                return ToString();
            }
        }
    }
}
