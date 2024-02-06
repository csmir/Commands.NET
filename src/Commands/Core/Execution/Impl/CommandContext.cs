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
