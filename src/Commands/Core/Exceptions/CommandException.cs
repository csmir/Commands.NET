namespace Commands
{
    /// <summary>
    ///     An exception thrown anywhere in the command execution pipeline.
    /// </summary>
    public class CommandException(string message, Exception? innerException = null) : Exception(message, innerException)
    {

    }
}
