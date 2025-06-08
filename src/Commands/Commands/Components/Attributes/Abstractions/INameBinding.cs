namespace Commands;

/// <summary>
///     An interface that defines names of a target component, such as a command module, command, or parameter.
/// </summary>
public interface INameBinding
{
    /// <summary>
    ///     Gets the name of the target.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the names of the target.
    /// </summary>
    /// <remarks>
    ///     Names are not considered for parameter names. Parameters have only one name, defined by <see cref="Name"/>.
    /// </remarks>
    public string[] Names { get; }
}
