namespace Commands
{
    /// <summary>
    ///     A <see cref="CommandException"/> which is thrown when a component fails to be built, modified, or accessed.
    /// </summary>
    /// <param name="message">The failure message which caused the component to reject an operation.</param>
    /// <param name="innerException">An inner exception that occurred during exception; or null if none occurred.</param>
    public class BuildException(string message, Exception? innerException = null)
        : CommandException(message, innerException)
    {
        const string SEALED_COMPONENT_ACCESS_ERROR = "Cannot access a sealed component.";

        internal static BuildException AccessDenied()
            => new(SEALED_COMPONENT_ACCESS_ERROR);
    }
}
