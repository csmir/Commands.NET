namespace Commands;

/// <summary>
///     Represents an exception that is thrown when a command route is incomplete.
/// </summary>
public sealed class CommandRouteIncompleteException(IComponent discoveredComponent)
    : Exception
{
    /// <summary>
    ///     Gets the discovered component.
    /// </summary>
    public IComponent Component { get; } = discoveredComponent;
}