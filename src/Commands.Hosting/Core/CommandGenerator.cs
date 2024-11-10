using Commands.Resolvers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Commands
{
    /// <summary>
    ///     A generator for command execution scopes, listening to data within the provided <paramref name="resolvers"/> to run a new command.
    /// </summary>
    /// <param name="manager">The manager used to run the command query.</param>
    /// <param name="resolvers">A collection of registered resolvers intended to be activated.</param>
    /// <param name="logger">A logger that logs the execution process.</param>
    /// <param name="lifetime">The lifetime of the application.</param>
    public sealed class CommandGenerator(
        CommandManager manager, IEnumerable<SourceResolverBase> resolvers, ILogger<CommandGenerator> logger, IHostApplicationLifetime lifetime)
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
            lifetime.ApplicationStarted.Register(() =>
            {
                foreach (var resolver in _resolvers)
                {
                    resolver.ResourceAvailable = true;
                }
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                foreach (var resolver in _resolvers)
                {
                    resolver.ResourceAvailable = false;
                }
            });

            await Task.CompletedTask;

            _ = RunAsync();
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        ///     Makes an attempt to pause command input entering the application flow, and returns a value indicating whether the operation was successful.
        /// </summary>
        /// <returns><see langword="true"/> if all source resolvers were succesfully paused. <see langword="false"/> if one or more failed to pause.</returns>
        public bool TryPause()
        {
            try
            {
                foreach (var resolvers in _resolvers)
                {
                    resolvers.ResourceAvailable = false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Makes an attempt to unpause command input entering the application flow, and returns a value indicating whether the operation was successful.
        /// </summary>
        /// <returns><see langword="true"/> if all source resolvers succesfully unpaused. <see langword="false"/> if one or more failed to unpause.</returns>
        public bool TryUnpause()
        {
            try
            {
                foreach (var resolvers in _resolvers)
                {
                    resolvers.ResourceAvailable = true;
                }
            }
            catch
            {
                return false;
            }

            return true;
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
                        var source = await resolver.Evaluate(cToken);

                        if (!source.Success)
                        {
                            _logger.LogWarning("Source resolver failed to succeed acquirement iteration.");

                            if (source.Exception != null)
                            {
                                throw source.Exception;
                            }

                            continue;
                        }

                        var options = source.Options ?? new();

                        options.AsyncMode = AsyncMode.Await;

                        await _manager.Execute(source.Consumer!, source.Args!, options); // never null if source succeeded.
                    }
                });
            }
        }
    }
}
