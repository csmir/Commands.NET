namespace Commands.Core
{
    /// <summary>
    ///     The result of a check operation within the command execution pipeline.
    /// </summary>
    public readonly struct ConditionResult : ICommandResult
    {
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

        private ConditionResult(Exception? exception)
        {
            Exception = exception;
        }

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> resembling a successful check operation.
        /// </summary>
        /// <returns>A new result containing information about the operation.</returns>
        public static ConditionResult FromSuccess()
        {
            return new(null);
        }

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> resembling a failed check operation.
        /// </summary>
        /// <param name="exception">The exception that occurred during the check operation.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static ConditionResult FromError(Exception exception)
        {
            return new(exception);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Success = {(Exception == null ? "True" : $"False \nException = {Exception}")}";
        }
    }
}
