using System.Diagnostics;

namespace Commands
{
    /// <summary>
    ///     The result of a convert operation within the command execution pipeline.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public readonly struct ConvertResult : IValueResult
    {
        /// <inheritdoc />
        public object? Value { get; }

        /// <inheritdoc />
        public Exception? Exception { get; }

        /// <inheritdoc />
        public bool Success
            => Exception == null;

        private ConvertResult(Exception? exception, object? value)
        {
            Exception = exception;
            Value = value;
        }

        /// <summary>
        ///     Creates a new <see cref="ConvertResult"/> resembling a successful conversion operation.
        /// </summary>
        /// <remarks>
        ///     This overload is called when conversion succeeds with a value.
        /// </remarks>
        /// <param name="value">The converted value of the operation.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static ConvertResult FromSuccess(object? value)
            => new(null, value);

        /// <summary>
        ///     Creates a new <see cref="ConvertResult"/> resembling a failed conversion operation.
        /// </summary>
        /// <param name="exception">The exception that occurred during the conversion operation.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static ConvertResult FromError(Exception exception)
            => new(exception, null);

        /// <summary>
        ///     Creates a new <see cref="ConvertResult"/> resembling a failed conversion operation, containing a mismatch of arguments.
        /// </summary>
        /// <returns></returns>
        public static ConvertResult FromError()
            => new(ConvertException.ConvertFailed(null), null);

        /// <inheritdoc />
        public override string ToString()
            => $"Success = {(Exception == null ? "True" : $"False \nException = {Exception.Message}")}";

        /// <summary>
        ///     Gets a string representation of this result.
        /// </summary>
        /// <param name="inline">Sets whether the string representation should be inlined or not.</param>
        /// <returns></returns>
        public string ToString(bool inline)
            => inline ? $"Success = {(Exception == null ? "True" : $"False")}" : ToString();

        /// <summary>
        ///     Implicitly converts a <see cref="ConvertResult"/> to a <see cref="Task{TResult}"/>.
        /// </summary>
        /// <param name="result">The result to convert.</param>
        public static implicit operator Task<ConvertResult>(ConvertResult result)
            => Task.FromResult(result);
    }
}
