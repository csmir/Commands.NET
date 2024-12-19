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

        /// <summary>
        ///     Gets the result of command execution, being the returned value by the executed method.
        /// </summary>
        /// <remarks>
        ///     This property will be <see langword="null"/> when the executed method returns <see langword="void"/> or <see langword="null"/>. Any other returned value will be represented as a non-null value.
        /// </remarks>
        public object? Result { get; }

        /// <inheritdoc />
        public Exception? Exception { get; }

        /// <inheritdoc />
        public bool Success
            => Exception == null;

        private InvokeResult(CommandInfo command, object? result, Exception? exception)
        {
            Command = command;
            Result = result;

            Exception = exception;
        }

        /// <summary>
        ///     Creates a new <see cref="InvokeResult"/> resembling a successful invocation operation.
        /// </summary>
        /// <param name="command">The command that was invoked.</param>
        /// <param name="result">The result of command execution, being the returned value by the executed method.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static InvokeResult FromSuccess(CommandInfo command, object? result)
            => new(command, result, null);

        /// <summary>
        ///     Creates a new <see cref="InvokeResult"/> resembling a failed invocation operation.
        /// </summary>
        /// <param name="command">The command that failed to be invoked</param>
        /// <param name="exception">The exception that occurred during command invocation.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static InvokeResult FromError(CommandInfo command, Exception exception)
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
            => inline ? $"Success = {(Exception == null ? "True" : $"False")}" : ToString();

        /// <summary>
        ///     Implicitly converts a <see cref="InvokeResult"/> to a <see cref="ValueTask{TResult}"/>.
        /// </summary>
        /// <param name="result">The result to convert.</param>
        public static implicit operator ValueTask<InvokeResult>(InvokeResult result)
            => new(result);
    }
}
