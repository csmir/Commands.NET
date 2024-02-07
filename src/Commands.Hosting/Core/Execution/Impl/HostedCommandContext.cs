using Microsoft.Extensions.Logging;

namespace Commands.Core
{
    /// <summary>
    ///     An implementation of <see cref="ConsumerBase"/> that is configured to use an <see cref="ILogger"/> to resolve log messages.
    /// </summary>
    /// <param name="logger">The logger to use as a resolver for log messages.</param>
    public class HostedCommandContext(ILogger logger) : ConsumerBase
    {
        /// <summary>
        ///     Gets the logger used to resolve log messages.
        /// </summary>
        public ILogger Logger { get; } = logger;
    }
}
