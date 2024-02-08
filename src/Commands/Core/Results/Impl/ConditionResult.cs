using System.Diagnostics;

namespace Commands.Core
{
    /// <summary>
    ///     The result of a check operation within the command execution pipeline.
    /// </summary>
    [DebuggerDisplay("Success = {Success()}")]
    public readonly struct ConditionResult : IRunResult
    {
        /// <inheritdoc />
        public Exception Exception { get; } = null;

        internal ConditionResult(Exception exception)
        {
            Exception = exception;
        }

        /// <inheritdoc />
        public bool Success()
        {
            return Exception == null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Success = {(Exception == null ? "True" : $"False \nException = {Exception}")}";
        }
    }
}
