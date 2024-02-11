using Commands.Reflection;
using Commands.TypeConverters;

namespace Commands.Core
{
    /// <summary>
    ///     The result of a convert operation within the command execution pipeline.
    /// </summary>
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

        internal object? Value { get; }

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
        public static ConvertResult FromSuccess(object value)
        {
            return new(null, value);
        }

        /// <summary>
        ///     Creates a new <see cref="ConvertResult"/> resembling a successful conversion operation.
        /// </summary>
        /// <remarks>
        ///     This overload is called when conversion succeeds with a null value. This should not be called when implementing <see cref="TypeConverterBase.EvaluateAsync(ConsumerBase, IArgument, string, IServiceProvider, CancellationToken)"/>.
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
            return $"Success = {(Exception == null ? "True" : $"False \nException = {Exception}")}";
        }
    }
}
