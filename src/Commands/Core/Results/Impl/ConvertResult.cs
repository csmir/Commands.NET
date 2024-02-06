namespace Commands
{
    /// <summary>
    ///     The result of a convert operation within the command execution pipeline.
    /// </summary>
    public readonly struct ConvertResult : ICommandResult
    {
        /// <inheritdoc />
        public Exception Exception { get; } = null;

        internal object Value { get; } = null;

        internal ConvertResult(object value)
        {
            Value = value;
        }

        internal ConvertResult(Exception exception)
        {
            Exception = exception;
        }

        /// <inheritdoc />
        public bool Success()
        {
            return Exception == null;
        }
    }
}
