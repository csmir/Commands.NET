﻿namespace Commands;

/// <summary>
///     An <see cref="Exception"/> that is thrown when no command could be found. This class cannot be inherited.
/// </summary>
/// <param name="message">The message that represents the reason of the exception being thrown.</param>
/// <param name="innerException">An exception thrown by an inner operation, if present.</param>
public sealed class SearchException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    const string COMPONENTS_NOT_FOUND = "No commands were discovered with the provided input.";
    const string SEARCH_INCOMPLETE = $"A {nameof(CommandModule)} was discovered, but it contained no executable targets with the provided input.";

    internal static SearchException ComponentsNotFound()
        => new(COMPONENTS_NOT_FOUND);

    internal static SearchException SearchIncomplete()
        => new(SEARCH_INCOMPLETE);
}
