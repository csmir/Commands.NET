namespace Commands
{
    /// <summary>
    ///     The result of a check operation within the command execution pipeline.
    /// </summary>
    public readonly struct CheckResult : ICommandResult
    {
        /// <inheritdoc />
        public Exception Exception { get; } = null;

        /// <inheritdoc />
        public bool Success { get; } = true;

        internal CheckResult(Exception exception)
        {
            Exception = exception;
            Success = false;
        }

        internal CheckResult(bool success)
        {
            Success = success;
        }
    }
}
