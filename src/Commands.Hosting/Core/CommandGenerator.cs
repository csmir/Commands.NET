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
            await Task.CompletedTask;

            _ = RunAsync();
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        private Task RunAsync()
        {
            var tasks = ActivateResolvers();

            return Task.WhenAll(tasks.Select(x =>
            {
                x.Start();

                return x;
            }));
        }

        private IEnumerable<Task> ActivateResolvers()
        {
            foreach (var resolver in _resolvers)
            {
                var cToken = CancellationSource.Token;

                yield return new Task(async () =>
                {
                    while (!cToken.IsCancellationRequested)
                    {
                        var source = await resolver.EvaluateAsync(cToken);

                        if (!source.Success)
                        {
                            _logger.LogWarning("Source resolver failed to succeed acquirement iteration.");

                            continue;
                        }

                        var options = source.Options ?? new();

                        options.AsyncMode = AsyncMode.Await;

                        await _manager.TryExecuteAsync(source.Consumer!, source.Args!, options); // never null if source succeeded.
                    }
                });
            }
        }
    }
}
