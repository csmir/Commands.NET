﻿using Commands.Conversion;

namespace Commands
{
    /// <summary>
    ///     An <see cref="Exception"/> that is thrown when no matched command succeeded converting its arguments. This class cannot be inherited.
    /// </summary>
    /// <param name="message">The message that represents the reason of the exception being thrown.</param>
    /// <param name="innerException">An exception thrown by an inner operation, if present.</param>
    public sealed class ConvertException(string message, Exception? innerException = null)
        : Exception(message, innerException)
    {
        const string CONVERTER_FAILED = $"A {nameof(TypeParser)} failed to parse the provided value as '{{0}}'. View inner exception for more details.";
        const string ARGUMENT_MISMATCH = "An argument mismatch occurred between the best target and the input value.";

        internal static ConvertException ConvertFailed(Type? type, Exception? innerException = null)
            => new(string.Format(CONVERTER_FAILED, type?.Name ?? "Unknown"), innerException);

        internal static ConvertException ArgumentMismatch()
            => new(ARGUMENT_MISMATCH);
    }
}
