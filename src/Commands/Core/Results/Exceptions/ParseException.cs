using Commands.Conversion;

namespace Commands
{
    /// <summary>
    ///     An <see cref="Exception"/> that is thrown when no matched command succeeded converting its arguments. This class cannot be inherited.
    /// </summary>
    /// <param name="message">The message that represents the reason of the exception being thrown.</param>
    /// <param name="innerException">An exception thrown by an inner operation, if present.</param>
    public sealed class ParseException(string message, Exception? innerException = null)
        : Exception(message, innerException)
    {
        const string CONVERTER_FAILED = $"A {nameof(TypeParser)} failed to parse the provided value as '{{0}}'.";
        const string ARGUMENT_MISMATCH = "An argument mismatch occurred between the best target and the input value. Target length: {0}. Input count: {1}";

        internal static ParseException ConvertFailed(Type? type, Exception? innerException = null)
            => new(string.Format(CONVERTER_FAILED, type?.Name ?? "Unknown"), innerException);

        internal static ParseException ArgumentMismatch(int cmdLength, int inputLength)
            => new(string.Format(ARGUMENT_MISMATCH, cmdLength, inputLength));
    }
}
