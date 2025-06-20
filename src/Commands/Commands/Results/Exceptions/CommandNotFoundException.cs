namespace Commands;

/// <summary>
///     Represents an exception that is created when a command is not found.
/// </summary>
public sealed class CommandNotFoundException()
    : Exception("No command was found matching the provided request. This can happen when the command is not registered, or when the request is malformed.")
{
}
