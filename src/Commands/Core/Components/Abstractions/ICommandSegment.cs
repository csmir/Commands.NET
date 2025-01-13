namespace Commands;

/// <summary>
///     Reveals a name and potential attributes of a component necessary for execution.
/// </summary>
public interface ICommandSegment : IComparable
{
    /// <summary>
    ///     Gets the name of the component.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    ///     Gets all attributes on the current object.
    /// </summary>
    public Attribute[] Attributes { get; }

    /// <summary>
    ///     Builds the full name of the component.
    /// </summary>
    /// <returns>A string representing the full name of the component.</returns>
    public string GetFullName();

    /// <summary>
    ///     Builds the score of the component.
    /// </summary>
    /// <returns>A float representing the score of the component.</returns>
    public float GetScore();
}
