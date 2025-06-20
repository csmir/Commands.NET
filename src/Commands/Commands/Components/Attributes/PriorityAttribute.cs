namespace Commands;

/// <summary>
///     An attribute that can prioritize one result over another when multiple matches were found. This class cannot be inherited.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class PriorityAttribute : Attribute
{
    /// <summary>
    ///     Gets the priority of a command, where higher values take priority over lower ones.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PriorityAttribute"/> class with the specified priority.
    /// </summary>
    /// <param name="priority">The priority of the implementation, which influences ordering a sequence of similar components to prioritize one over the other.</param>
    public PriorityAttribute(short priority)
        => Priority = priority;

    // An overload for int to allow for larger priority values than user implementations, so the library can ensure certain targets are executed first or last.
    internal PriorityAttribute(int priority)
        => Priority = priority;
}
