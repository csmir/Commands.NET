using Commands.Reflection;
using System.Diagnostics;

namespace Commands
{
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

        /// <summary>
        ///     Creates a new <see cref="SearchResult"/> resembling a successful search operation.
        /// </summary>
        /// <param name="component">The discovered component.</param>
        /// <param name="searchHeight">The argument index of the discovered component.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SearchResult FromSuccess(IComponent component, int searchHeight)
            => new(component, searchHeight, null);

        /// <summary>
        ///     Creates a new <see cref="SearchResult"/> resembling a failed search operation.
        /// </summary>
        /// <remarks>
        ///     This overload is called when searching discovers a module but no commands to target.
        /// </remarks>
        /// <param name="module">The module that was found, of which no commands were parsed.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SearchResult FromError(ModuleInfo module)
            => new(module, 0, SearchException.SearchIncomplete());

        /// <summary>
        ///     Creates a new <see cref="SearchResult"/> resembling a failed search operation.
        /// </summary>
        /// <remarks>
        ///     This overload is called when searching throws an error.
        /// </remarks>
        /// <param name="exception">The exception that caused the search to fail.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SearchResult FromError(Exception exception)
            => new(null, 0, exception);

        /// <summary>
        ///     Creates a new <see cref="SearchResult"/> resembling a failed search operation.
        /// </summary>
        /// <remarks>
        ///     This overload is called when no commands or modules are found.
        /// </remarks>
        /// <returns>A new result containing information about the operation.</returns>
        public static SearchResult FromError()
            => new(null, 0, SearchException.ComponentsNotFound());

        /// <inheritdoc />
        public override string ToString()
            => $"{(Component != null ? $"Component = {Component} \n" : "")}Success = {(Exception == null ? "True" : $"False \nException = {Exception.Message}")}";

        /// <summary>
        ///     Gets a string representation of this result.
        /// </summary>
        /// <param name="inline">Sets whether the string representation should be inlined or not.</param>
        /// <returns></returns>
        public string ToString(bool inline)
            => inline ? $"{(Component != null ? $"Component = {Component} " : "")}Success = {(Exception == null ? "True" : $"False")}" : ToString();

        /// <summary>
        ///     Implicitly converts a <see cref="SearchResult"/> to a <see cref="ValueTask{TResult}"/>.
        /// </summary>
        /// <param name="result">The result to convert.</param>
        public static implicit operator ValueTask<SearchResult>(SearchResult result)
            => new(result);
    }
}
