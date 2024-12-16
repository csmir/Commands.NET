namespace Commands
{
    /// <summary>
    ///     An exception thrown anywhere in the command execution or creation pipeline.
    /// </summary>
    public class CommandException(string message, Exception? innerException = null) : Exception(message, innerException)
    {

    }
}
