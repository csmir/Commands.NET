using Commands.Reflection;
using System.Diagnostics;

namespace Commands.Core
{
    /// <summary>
    ///     The result of an invocation operation within the command execution pipeline.
    /// </summary>
    [DebuggerDisplay("Success = {Success()}")]
    public readonly struct InvokeResult : IRunResult
    {
        /// <inheritdoc />
        public Exception Exception { get; } = null;

        /// <summary>
        ///     Gets the command responsible for the invocation.
        /// </summary>
        public CommandInfo Command { get; }

        internal InvokeResult(CommandInfo command, Exception exception)
        {
            Exception = exception;
            Command = command;
        }

        internal InvokeResult(CommandInfo command)
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
            return $"Command = {Command} \nSuccess = {(Exception == null ? "True" : $"False \nException = {Exception}")}";
        }
    }
}
