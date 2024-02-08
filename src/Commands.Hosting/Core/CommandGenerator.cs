using Commands.Resolvers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Commands.Core
{
    /// <summary>
    ///     A generator for command execution scopes, listening to data within the provided <paramref name="resolvers"/> to run a new command.
    /// </summary>
    /// <param name="manager">The manager used to run the command query.</param>
    /// <param name="resolvers">A collection of registered resolvers intended to be activated.</param>
    /// <param name="logger">A logger that logs the execution process.</param>
    public sealed class CommandGenerator(
        CommandManager manager, IEnumerable<SourceResolverBase> resolvers, ILogger<CommandGenerator> logger)
        : IHostedService
    {
        private readonly ILogger<CommandGenerator> _logger = logger;
        private readonly CommandManager _manager = manager;
        private readonly IEnumerable<SourceResolverBase> _resolvers = resolvers;

        /// <summary>
        ///     Gets a token source that can cancel the parallel source loop.
        /// </summary>
        public CancellationTokenSource CancellationSource { get; } = new();

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await RunAsync();
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        private async Task RunAsync()
        {
            await Parallel.ForEachAsync(_resolvers, CancellationSource.Token, async (resolver, cToken) =>
            {
                while (!cToken.IsCancellationRequested)
                {
                    var source = await resolver.EvaluateAsync(cToken);

                    if (!source.Success)
                    {
                        _logger.LogWarning("Source resolver failed to succeed acquirement iteration.");
                    }

                    var options = source.Options ?? new();

                    options.AsyncMode = AsyncMode.Discard;

                    await _manager.TryExecuteAsync(source.Consumer, source.Args, options);
                }
            });
        }
    }
}
