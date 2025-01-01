namespace Commands
{
    /// <summary>
    ///     The result of an operation within the command execution pipeline.
    /// </summary>
    public interface IExecuteResult
    {
        /// <summary>
        ///     Gets the exception that represents the reason and context of a failed operation.
        /// </summary>
        /// <remarks>
        ///     Will be <see langword="null"/> if <see cref="Success"/> returns <see langword="true"/>.
        /// </remarks>
        public Exception? Exception { get; }

        /// <summary>
        ///     Gets if the result was successful or not.
        /// </summary>
        /// <returns><see langword="true"/> if this result represents a successful operation, otherwise <see langword="false"/>.</returns>
        /// <inheritdoc />
        public bool Success { get; }
    }
}
