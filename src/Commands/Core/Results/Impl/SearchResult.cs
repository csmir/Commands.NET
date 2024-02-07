using Commands.Reflection;

namespace Commands.Core
{
    /// <summary>
    ///     The result of a search operation within the command execution pipeline.
    /// </summary>
    public readonly struct SearchResult : ICommandResult
    {
        /// <inheritdoc />
        public Exception Exception { get; } = null;

        /// <summary>
        ///     Gets the command that was found for this result.
        /// </summary>
        /// <remarks>
        ///     Will be <see langword="null"/> if <see cref="Success"/> returns <see langword="false"/>.
        /// </remarks>
        public CommandInfo Command { get; } = null;

        internal int SearchHeight { get; }

        internal SearchResult(CommandInfo command, int srcHeight)
        {
            Command = command;
            SearchHeight = srcHeight;
        }

        internal SearchResult(Exception exception)
        {
            Exception = exception;
            SearchHeight = 0;
        }

        /// <inheritdoc />
        public bool Success()
        {
            return Exception == null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Success = {(Exception == null ? "True" : $"False: {Exception}")}";
        }
    }
}
