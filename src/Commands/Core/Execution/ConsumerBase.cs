using System.Diagnostics.CodeAnalysis;

namespace Commands.Core
{
    /// <summary>
    ///     Represents the data about the consumer of the command.
    /// </summary>
    public class ConsumerBase
    {
        private readonly object _lock = new();
        private ICommandResult _fallback;

        internal bool TryGetFallback([NotNullWhen(true)] out ICommandResult result)
        {
            lock (_lock)
            {
                result = _fallback;

                return _fallback != null;
            }
        }

        internal void TrySetFallback(ICommandResult result)
        {
            lock (_lock)
            {
                _fallback ??= result;
            }
        }
    }
}
