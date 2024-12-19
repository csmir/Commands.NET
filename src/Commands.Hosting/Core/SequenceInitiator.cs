using Commands.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Commands
{
    /// <summary>
    ///     A generator for command execution scopes, listening to data within the provided <see cref="SourceResolver"/>[] <paramref name="resolvers"/> to run a new command. This class cannot be inherited.
    /// </summary>
    /// <param name="tree">The tree used to run the command query.</param>
    /// <param name="resolvers">A collection of registered resolvers intended to be activated.</param>
    /// <param name="services">The services that the initiator uses to create a scoped flow for the execution pipeline.</param>
    /// <param name="logger">A logger that logs the execution process.</param>
    public sealed class SequenceInitiator(
        IComponentTree tree, IEnumerable<SourceResolver> resolvers, IServiceProvider services, ILogger<SequenceInitiator> logger)
        : ISequenceInitiator
    {
        private readonly ILogger<SequenceInitiator> _logger = logger;
        private readonly IServiceProvider _services = services;

        /// <inheritdoc />
        public CancellationTokenSource CancellationSource { get; } = new();

        /// <inheritdoc />
        public IComponentTree Tree { get; } = tree;

        /// <inheritdoc />
        public IEnumerable<SourceResolver> Resolvers { get; } = resolvers;

        /// <inheritdoc />
        public void TryStart()
        {
            foreach (var resolver in Resolvers)
                resolver.ReadAvailable = true;

            _ = RunAsync();
        }

        /// <inheritdoc />
        public void TryStop()
        {
            foreach (var resolver in Resolvers)
                resolver.ReadAvailable = false;
        }

        /// <inheritdoc />
        public bool TryPause()
        {
            try
            {
                foreach (var resolvers in Resolvers)
                    resolvers.ReadAvailable = false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public bool TryUnpause()
        {
            try
            {
                foreach (var resolvers in Resolvers)
                    resolvers.ReadAvailable = true;
            }
            catch
            {
                return false;
            }

            return true;
        }

        private Task RunAsync()
        {
            IEnumerable<Task> ActivateResolvers()
            {
                foreach (var resolver in Resolvers)
                {
                    var cToken = CancellationSource.Token;

                    yield return new Task(async () =>
                    {
                        while (!cToken.IsCancellationRequested)
                        {
                            var source = await resolver.Evaluate(_services, cToken);

                            if (!source.Success)
                            {
                                _logger.LogWarning("Source resolver failed to succeed acquirement iteration.");

                                if (source.Exception != null)
                                    throw source.Exception;

                                continue;
                            }

                            using var scope = _services.CreateAsyncScope();

                            var options = source.Options ?? new();

                            options.DoAsynchronousExecution = false;
                            options.Services = scope.ServiceProvider;

                            await Tree.Execute(source.Consumer!, source.Args!.Value, options); // never null if source succeeded.

                            await scope.DisposeAsync();
                        }
                    });
                }
            }

            return Task.WhenAll(ActivateResolvers().Select(x =>
            {
                x.Start();

                return x;
            }));
        }
    }
}
