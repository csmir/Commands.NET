﻿namespace Commands
{
    /// <summary>
    ///     An <see cref="CommandException"/> that is thrown when no command could be found.
    /// </summary>
    /// <param name="message">The message that represents the reason of the exception being thrown.</param>
    /// <param name="innerException">An exception thrown by an inner operation, if present.</param>
    public sealed class SearchException(string message, Exception? innerException = null)
        : CommandException(message, innerException)
    {
        const string COMPONENTS_NOT_FOUND = "No commands were found with the provided input.";
        const string SEARCH_INCOMPLETE = "A module was found with provided input, but no command to target was discovered.";

        internal static SearchException ComponentsNotFound()
            => new SearchException(COMPONENTS_NOT_FOUND);

        internal static SearchException SearchIncomplete()
            => new SearchException(SEARCH_INCOMPLETE);
    }
}
