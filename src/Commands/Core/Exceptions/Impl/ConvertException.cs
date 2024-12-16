using Commands.Converters;

namespace Commands
{
    /// <summary>
    ///     An <see cref="CommandException"/> that is thrown when no matched command succeeded converting its arguments.
    /// </summary>
    /// <param name="message">The message that represents the reason of the exception being thrown.</param>
    /// <param name="innerException">An exception thrown by an inner operation, if present.</param>
    public sealed class ConvertException(string message, Exception? innerException = null)
        : CommandException(message, innerException)
    {
        const string CONVERTER_FAILED = $"A {nameof(TypeConverter)} failed to parse the provided value as '{{0}}'. View inner exception for more details.";
        const string ARGUMENT_MISMATCH = "An argument mismatch occurred between the best target and the input value.";

        internal static ConvertException ConvertFailed(Type type, Exception? innerException = null)
            => new(string.Format(CONVERTER_FAILED, type.Name), innerException);

        internal static ConvertException ArgumentMismatch()
            => new(ARGUMENT_MISMATCH);
    }
}
