namespace Commands;

/// <summary>
///     An <see cref="Exception"/> that is thrown when no matched command succeeded converting its arguments. This class cannot be inherited.
/// </summary>
/// <param name="message">The message that represents the reason of the exception being thrown.</param>
/// <param name="innerException">An exception thrown by an inner operation, if present.</param>
public sealed class ParseException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    const string PARSE_FAILED = $"A {nameof(TypeParser)} failed to parse the provided value as '{{0}}'.";
    const string ARGUMENT_MISMATCH = "Provided input is out of range of the target. Min: {0}, Max: {1}, Input: {2}.";

    internal static ParseException ParseFailed(Type? type, Exception? innerException = null)
        => new(string.Format(PARSE_FAILED, type?.Name ?? "Unknown"), innerException);

    internal static ParseException InputOutOfRange(int minlength, int maxlength, int inputLength)
        => new(string.Format(ARGUMENT_MISMATCH, minlength, maxlength, inputLength));
}
