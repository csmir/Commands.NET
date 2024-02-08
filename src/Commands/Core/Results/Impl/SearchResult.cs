using Commands.Exceptions;
using Commands.Reflection;
using System.Diagnostics;

namespace Commands.Core
{
    /// <summary>
    ///     The result of a search operation within the command execution pipeline.
    /// </summary>
    [DebuggerDisplay("Success = {Success()}")]
    public readonly struct SearchResult : IRunResult
    {
        /// <inheritdoc />
        public Exception Exception { get; } = null;

        /// <inheritdoc />
        public bool Success
        {
            get
            {
                return Exception == null;
            }
        }

        /// <summary>
        ///     Gets the command that was found for this result.
        /// </summary>
        public IConditional Component { get; } = null;

        internal int SearchHeight { get; }

        internal SearchResult(IConditional command, int srcHeight)
        {
            Component = command;
            SearchHeight = srcHeight;
        }

        internal SearchResult(ModuleInfo module)
        {
            SearchHeight = 0;

            Component = module;
            Exception = SearchException.Incomplete();
        }

        internal SearchResult(Exception exception)
        {
            SearchHeight = 0;

            Exception = exception;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{(Component != null ? $"Command = {Component} \n" : "")}Success = {(Exception == null ? "True" : $"False \nException = {Exception}")}";
        }
    }
}
