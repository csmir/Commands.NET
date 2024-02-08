namespace Commands.Exceptions
{
    /// <summary>
    ///     An <see cref="ExecutionException"/> that is thrown when no matched command succeeded converting its arguments.
    /// </summary>
    /// <param name="message">The message that represents the reason of the exception being thrown.</param>
    /// <param name="innerException">An exception thrown by an inner operation, if present.</param>
    public sealed class ConvertException(string message, Exception? innerException = null)
        : ExecutionException(message, innerException)
    {
        const string CONVERTER_FAILED = "TypeConverter failed to parse provided value as '{0}'. View inner exception for more details.";
        const string TOO_SHORT = "Query is too short for best match.";
        const string TOO_LONG = "Query is too long for best match.";

        internal static ConvertException ConvertFailed(Type type, Exception innerException)
        {
            return new(string.Format(CONVERTER_FAILED, type), innerException);
        }

        internal static ConvertException InputTooLong()
        {
            return new(TOO_LONG);
        }

        internal static ConvertException InputTooShort()
        {
            return new(TOO_SHORT);
        }
    }
}
