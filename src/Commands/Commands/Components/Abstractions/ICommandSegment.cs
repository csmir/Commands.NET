namespace Commands;

/// <summary>
///     Reveals information about a segment related to a component, which is necessary for execution.
/// </summary>
public interface ICommandSegment : IComparable
{
    /// <summary>
    ///     Gets the name of the segment.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    ///     Gets all attributes on the segment.
    /// </summary>
    public Attribute[] Attributes { get; }

    /// <summary>
    ///     Returns the full name of the segment.
    /// </summary>
    /// <returns>A string representing the full name of the segment.</returns>
    public string GetFullName();

    /// <summary>
    ///     Returns the score of the segment.
    /// </summary>
    /// <returns>A float representing the score of the segment.</returns>
    public float GetScore();
}
