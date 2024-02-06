using Microsoft.Extensions.Logging;

namespace Commands.Core
{
#pragma warning disable CA2254 // Template should be a static expression
    /// <summary>
    ///     An implementation of <see cref="CommandContext"/> that is configured to use an <see cref="ILogger"/> to resolve log messages.
    /// </summary>
    /// <param name="logger">The logger to use as a resolver for log messages.</param>
    public class HostedCommandContext(ILogger logger) : CommandContext
    {
        /// <summary>
        ///     Gets the logger used to resolve log messages.
        /// </summary>
        public ILogger Logger { get; } = logger;

        /// <inheritdoc />
        public override void LogTrace(string message, params object[] args)
        {
            Logger.Log(logLevel: LogLevel.Trace, message: message, args: args);
        }

        /// <inheritdoc />
        public override void LogDebug(string message, params object[] args)
        {
            Logger.Log(LogLevel.Debug, message, args);
        }

        /// <inheritdoc />
        public override void LogInformation(string message, params object[] args)
        {
            Logger.Log(LogLevel.Information, message, args);
        }

        /// <inheritdoc />
        public override void LogWarning(string message, params object[] args)
        {
            Logger.Log(LogLevel.Warning, message, args);
        }

        /// <inheritdoc />
        public override void LogError(string message, params object[] args)
        {
            Logger.Log(LogLevel.Error, message, args);
        }

        /// <inheritdoc />
        public override void LogCritical(string message, params object[] args)
        {
            Logger.Log(LogLevel.Critical, message, args);
        }
    }
#pragma warning restore CA2254 // Template should be a static expression
}
