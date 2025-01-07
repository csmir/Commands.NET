namespace Commands;

/// <summary>
///     Represents an exception that is thrown when a command route is incomplete.
/// </summary>
public sealed class CommandRouteIncompleteException(CommandGroup discoveredGroup)
    : Exception(string.Join(MESSAGE, discoveredGroup))
{
    const string MESSAGE = "Command route is incomplete. Group {0} resolved no possible targets.";

    /// <summary>
    ///     Gets the group that caused the exception.
    /// </summary>
    public CommandGroup Group { get; } = discoveredGroup;
}