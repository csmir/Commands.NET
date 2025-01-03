namespace Commands;

/// <summary>
///     The result of a search operation within the command execution pipeline.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public readonly struct SearchResult : IExecuteResult
{
    /// <summary>
    ///     Gets the component that was discovered for this result.
    /// </summary>
    public IComponent? Component { get; }

    /// <inheritdoc />
    public Exception? Exception { get; }

    /// <inheritdoc />
    public bool Success
        => Exception == null;

    internal int SearchHeight { get; }

    private SearchResult(IComponent? component, int searchHeight, Exception? exception)
    {
        Component = component;
        SearchHeight = searchHeight;
        Exception = exception;
    }

    internal static SearchResult FromSuccess(IComponent component, int searchHeight)
        => new(component, searchHeight, null);

    internal static SearchResult FromError(CommandGroup? module = null)
        => new(module, 0, module != null ? SearchException.SearchIncomplete() : SearchException.ComponentsNotFound());

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
