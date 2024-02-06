namespace Commands
{
    /// <summary>
    ///     The result of any operation within the command execution pipeline.
    /// </summary>
    /// <remarks>
    ///     This interface encompasses a number of results that each represent a different step in the execution pipeline.
    ///     To deduce which 
    /// </remarks>
    public interface ICommandResult
    {
        /// <summary>
        ///     Gets the exception that represents the reason and context of a failed operation.
        /// </summary>
        /// <remarks>
        ///     Will be <see langword="null"/> if <see cref="Success"/> returns <see langword="true"/>.
        /// </remarks>
        public Exception Exception { get; }

        /// <summary>
        ///     Gets if the result was successful or not.
        /// </summary>
        public bool Success { get; }
    }
}
