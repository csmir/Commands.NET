using Commands.Converters;
using Commands.Reflection;
using System.Diagnostics;

namespace Commands
{
    /// <summary>
    ///     The result of a convert operation within the command execution pipeline.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public readonly struct ConvertResult : ICommandResult
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

        /// <summary>
        ///    The value of the conversion operation.
        /// </summary>
        public object? Value { get; }

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
        {
            return new(null, value);
        }

        /// <summary>
        ///     Creates a new <see cref="ConvertResult"/> resembling a successful conversion operation.
        /// </summary>
        /// <remarks>
        ///     This overload is called when conversion succeeds with a null value. This should not be called when implementing <see cref="TypeConverterBase.Evaluate(ConsumerBase, IArgument, object, IServiceProvider, CancellationToken)"/>.
        /// </remarks>
        /// <returns>A new result containing information about the operation.</returns>
        public static ConvertResult FromSuccess()
        {
            return new(null, null);
        }

        /// <summary>
        ///     Creates a new <see cref="ConvertResult"/> resembling a failed conversion operation.
        /// </summary>
        /// <param name="exception">The exception that occurred during the conversion operation.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static ConvertResult FromError(Exception exception)
        {
            return new(exception, null);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Success = {(Exception == null ? "True" : $"False \nException = {Exception.Message}")}";
        }

        /// <summary>
        ///     Gets a string representation of this result.
        /// </summary>
        /// <param name="inline">Sets whether the string representation should be inlined or not.</param>
        /// <returns></returns>
        public string ToString(bool inline)
        {
            if (inline)
            {
                return $"Success = {(Exception == null ? "True" : $"False")}";
            }
            else
            {
                return ToString();
            }
        }

        /// <summary>
        ///     Implicitly converts a <see cref="ConvertResult"/> to a <see cref="ValueTask{TResult}"/>.
        /// </summary>
        /// <param name="result">The result to convert.</param>
        public static implicit operator ValueTask<ConvertResult>(ConvertResult result)
        {
            return new(result);
        }
    }
}
