namespace Commands
{
    /// <summary>
    ///     The result of any operation in the command execution pipeline that holds a result.
    /// </summary>
    public interface IValueResult : IExecuteResult
    {
        /// <summary>
        ///     Gets the value of the resulted operation.
        /// </summary>
        /// <remarks>
        ///     Will be null when <see cref="IExecuteResult.Success"/> is <see langword="false"/>.
        /// </remarks>
        public object? Value { get; }
    }
}
