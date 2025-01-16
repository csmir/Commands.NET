namespace Commands;

/// <summary>
///     Represents an exception that is thrown when provided arguments are out of range of a command.
/// </summary>
public sealed class CommandOutOfRangeException(Command command, int argsLength)
    : Exception
{
    /// <summary>
    ///     Gets the command that caused the exception.
    /// </summary>
    public Command Command { get; } = command;

    /// <summary>
    ///     Gets the length of the arguments that caused the exception.
    /// </summary>
    public int ArgsLength { get; } = argsLength;
}
