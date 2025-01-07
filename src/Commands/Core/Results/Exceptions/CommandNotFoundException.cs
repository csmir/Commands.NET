namespace Commands;

/// <summary>
///     Represents an exception that is thrown when a command is not found.
/// </summary>
public sealed class CommandNotFoundException()
    : Exception(MESSAGE)
{
    const string MESSAGE = "No command was found with the provided input.";
}
