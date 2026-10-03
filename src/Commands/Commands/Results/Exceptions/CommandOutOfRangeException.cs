namespace Commands;

/// <summary>
///     Represents an exception that is created when provided arguments are out of range of a command.
/// </summary>
/// <remarks>
///     When the amount of arguments matches the command, but a required parameter did not receive a value, the inner exception is a <see cref="MissingArgumentException"/>.
/// </remarks>
public sealed class CommandOutOfRangeException(Command command, int argsLength, Exception? innerException = null)
    : Exception(null, innerException)
{
    /// <summary>
    ///     Gets the command that caused the exception.
    /// </summary>
    public Command Command { get; } = command;

    /// <summary>
    ///     Gets the length of the arguments that caused the exception.
    /// </summary>
    public int ArgumentLength { get; } = argsLength;
}
