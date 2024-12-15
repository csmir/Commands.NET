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
        const string CONVERTER_FAILED = "TypeConverter failed to parse provided value as '{0}'. View inner exception for more details.";
        const string ARGUMENT_MISMATCH = "Argument mismatch between best target and input.";

        internal static ConvertException ConvertFailed(Type type, Exception? innerException = null)
            => new(string.Format(CONVERTER_FAILED, type), innerException);

        internal static ConvertException ArgumentMismatch()
            => new(ARGUMENT_MISMATCH);
    }
}
