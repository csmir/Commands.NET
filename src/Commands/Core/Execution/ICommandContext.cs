using System.Diagnostics.CodeAnalysis;

namespace Commands.Core
{
    /// <summary>
    ///     A container that contains metadata and logging access for a command attempted to be executed.
    /// </summary>
    /// <remarks>
    ///     It is generally not adviced to implement this interface directly. Instead, consider implementing <see cref="CommandContext"/>.
    /// </remarks>
    public interface ICommandContext
    {
        /// <summary>
        ///     Creates and sends a trace log.
        /// </summary>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void LogTrace(string message, params object[] args);

        /// <summary>
        ///     Creates and sends a debug log.
        /// </summary>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void LogDebug(string message, params object[] args);

        /// <summary>
        ///     Creates and sends an information log.
        /// </summary>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void LogInformation(string message, params object[] args);

        /// <summary>
        ///     Creates and sends a warning log.
        /// </summary>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void LogWarning(string message, params object[] args);

        /// <summary>
        ///     Creates and sends an error log.
        /// </summary>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void LogError(string message, params object[] args);

        /// <summary>
        ///     Creates and sends a critical log.
        /// </summary>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void LogCritical(string message, params object[] args);

        internal bool TryGetFallback([NotNullWhen(true)] out ICommandResult result);

        internal void TrySetFallback(ICommandResult result);
    }
}
