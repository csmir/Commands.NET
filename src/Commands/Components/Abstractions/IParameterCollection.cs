namespace Commands;

/// <summary>
///     Reveals information about a bucket that contains zero-or-more arguments to resolve.
/// </summary>
public interface IParameterCollection
{
    /// <summary>
    ///     Gets the activator for this bucket.
    /// </summary>
    public IActivator Activator { get; }

    /// <summary>
    ///     Gets an array of arguments this bucket exposes.
    /// </summary>
    public ICommandParameter[] Parameters { get; }

    /// <summary>
    ///     Gets if this bucket has zero or more arguments.
    /// </summary>
    public bool HasParameters { get; }

    /// <summary>
    ///     Gets the minimum length of this bucket's arguments.
    /// </summary>
    public int MinLength { get; }

    /// <summary>
    ///     Gets the maximum length of this bucket's arguments.
    /// </summary>
    public int MaxLength { get; }
}
