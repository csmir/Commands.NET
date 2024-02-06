namespace Commands.Core
{
    /// <summary>
    ///     Represents the base implementation of <see cref="ICommandContext"/>.
    /// </summary>
    /// <remarks>
    ///     This class can be implemented to customize logging and command metadata.
    /// </remarks>
    public class CommandContext : ICommandContext
    {
        private readonly object _lock = new();
        private ICommandResult _fallback;

        /// <inheritdoc />
        /// <remarks>
        ///     This method can be overridden to provide an out-stream to log to.
        /// </remarks>
        public virtual void LogCritical(string message, params object[] args)
        {

        }

        /// <inheritdoc />
        /// <remarks>
        ///     This method can be overridden to provide an out-stream to log to.
        /// </remarks>
        public virtual void LogDebug(string message, params object[] args)
        {

        }

        /// <inheritdoc />
        /// <remarks>
        ///     This method can be overridden to provide an out-stream to log to.
        /// </remarks>
        public virtual void LogError(string message, params object[] args)
        {

        }

        /// <inheritdoc />
        /// <remarks>
        ///     This method can be overridden to provide an out-stream to log to.
        /// </remarks>
        public virtual void LogInformation(string message, params object[] args)
        {

        }

        /// <inheritdoc />
        /// <remarks>
        ///     This method can be overridden to provide an out-stream to log to.
        /// </remarks>
        public virtual void LogTrace(string message, params object[] args)
        {

        }

        /// <inheritdoc />
        /// <remarks>
        ///     This method can be overridden to provide an out-stream to log to.
        /// </remarks>
        public virtual void LogWarning(string message, params object[] args)
        {

        }

        bool ICommandContext.TryGetFallback(out ICommandResult result)
        {
            lock (_lock)
            {
                result = _fallback;

                return _fallback != null;
            }
        }

        void ICommandContext.TrySetFallback(ICommandResult result)
        {
            lock (_lock)
            {
                _fallback ??= result;
            }
        }
    }
}
