using Commands.Resolvers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Commands.Core
{
    public sealed class CommandRunner(
        CommandManager manager, SourceResolverBase resolver, ILogger logger)
        : IHostedService
    {
        private readonly ILogger _logger = logger;
        private readonly CommandManager _manager = manager;
        private readonly SourceResolverBase _resolver = resolver;

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await RunAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested)
            {
                var source = await _resolver.EvaluateAsync();

                if (!source.Success)
                {
                    _logger.LogWarning("Source resolver failed to succeed acquirement iteration.");
                    continue;
                }


            }
        }
    }
}
