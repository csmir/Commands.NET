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

    /// <summary>
    ///     Checks if the object has an attribute of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of attribute to check.</typeparam>
    /// <returns><see langword="true"/> if the object has an attribute of the specified type; otherwise, <see langword="false"/>.</returns>
    public bool HasAttribute<T>()
        where T : Attribute;

    /// <summary>
    ///     Gets the attribute of the specified type, or <paramref name="defaultValue"/> if it does not exist.
    /// </summary>
    /// <typeparam name="T">The type of attribute to get.</typeparam>
    /// <returns>The first attribute of the specified type on the current object, or <paramref name="defaultValue"/> if none were found.</returns>
    public T? GetAttribute<T>(T? defaultValue = default)
        where T : Attribute;
}
