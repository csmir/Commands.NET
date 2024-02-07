using Microsoft.Extensions.Logging;

namespace Commands.Core
{
    /// <inheritdoc />
    /// <remarks>
    ///     This implementation introduces support for logging at module-level.
    /// </remarks>
    public class HostedModuleBase<T> : ModuleBase<T>
        where T : HostedCommandContext
    {
        /// <summary>
        ///     Gets the logger with which log messages are sent.
        /// </summary>
        public ILogger Logger
        {
            get
                => Consumer.Logger;
        }
    }
}
