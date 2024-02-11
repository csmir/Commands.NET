using Commands.Exceptions;
using Commands.Reflection;

namespace Commands.Core
{
    /// <summary>
    ///     The result of a search operation within the command execution pipeline.
    /// </summary>
    public readonly struct SearchResult : ICommandResult
    {
        /// <summary>
        ///     Gets the component that was discovered for this result.
        /// </summary>
        public IConditional? Component { get; }

        /// <inheritdoc />
        public Exception? Exception { get; }

        /// <inheritdoc />
        public bool Success
        {
            get
            {
                return Exception == null;
            }
        }

        internal int SearchHeight { get; }

        private SearchResult(IConditional? component, int searchHeight, Exception? exception)
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
        public static SearchResult FromSuccess(IConditional component, int searchHeight)
        {
            return new(component, searchHeight, null);
        }

        /// <summary>
        ///     Creates a new <see cref="SearchResult"/> resembling a failed search operation.
        /// </summary>
        /// <remarks>
        ///     This overload is called when searching discovers a module but no commands to target.
        /// </remarks>
        /// <param name="module">The module that was found, of which no commands were parsed.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SearchResult FromError(ModuleInfo module)
        {
            return new(module, 0, SearchException.SearchIncomplete());
        }

        /// <summary>
        ///     Creates a new <see cref="SearchResult"/> resembling a failed search operation.
        /// </summary>
        /// <remarks>
        ///     This overload is called when searching throws an error.
        /// </remarks>
        /// <param name="exception">The exception that caused the search to fail.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SearchResult FromError(Exception exception)
        {
            return new(null, 0, exception);
        }

        /// <summary>
        ///     Creates a new <see cref="SearchResult"/> resembling a failed search operation.
        /// </summary>
        /// <remarks>
        ///     This overload is called when no commands or modules are found.
        /// </remarks>
        /// <returns>A new result containing information about the operation.</returns>
        public static SearchResult FromError()
        {
            return new(null, 0, SearchException.SearchNotFound());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{(Component != null ? $"Component = {Component} \n" : "")}Success = {(Exception == null ? "True" : $"False \nException = {Exception}")}";
        }
    }
}
