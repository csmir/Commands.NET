namespace Commands;

/// <summary>
///     The result of a search operation within the command execution pipeline.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public readonly struct SearchResult : IExecuteResult
{
    /// <summary>
    ///     Gets the component that was discovered for this result, if any. If no component was found, this will be <see langword="null"/>.
    /// </summary>
    public IComponent? Component { get; }

    /// <inheritdoc />
    public Exception? Exception { get; }

    /// <inheritdoc />
    public bool Success
        => Exception == null;

    /// <summary>
    ///     Gets the index at which the search operation ended and the command can be parsed from.
    /// </summary>
    public int ParseIndex { get; }

    private SearchResult(IComponent? component, int searchHeight, Exception? exception)
    {
        Component = component;
        ParseIndex = searchHeight;
        Exception = exception;
    }

    internal static SearchResult FromSuccess(IComponent component, int searchHeight)
        => new(component, searchHeight, null);

    internal static SearchResult FromError(CommandGroup group)
        => new(group, 0, new CommandRouteIncompleteException(group));

    internal static SearchResult FromError()
        => new(null, 0, new CommandNotFoundException());

    /// <inheritdoc />
    public override string ToString()
        => $"{(Component != null ? $"Component = {Component} \n" : "")}Success = {(Exception == null ? "True" : $"False \nException = {Exception.Message}")}";

    /// <summary>
    ///     Gets a string representation of this result.
    /// </summary>
    /// <param name="inline">Sets whether the string representation should be inlined or not.</param>
    /// <returns>A string containing a formatted value of the result.</returns>
    public string ToString(bool inline)
        => inline ? $"{(Component != null ? $"Component = {Component} " : "")}Success = {(Exception == null ? "True" : $"False")}" : ToString();

    /// <summary>
    ///     Implicitly converts a <see cref="SearchResult"/> to a <see cref="ValueTask{TResult}"/>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator ValueTask<SearchResult>(SearchResult result)
        => new(result);
}
