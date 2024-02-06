using Commands.Core;
using Commands.Helpers;
using Commands.Parsing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Commands.Tests
{
    public sealed class CommandHandler(CommandManager manager, ILoggerFactory logger) : IHostedService
    {
        private readonly ILoggerFactory _factory = logger;
        private readonly CommandManager _manager = manager;

        private readonly StringParser _parser = new();
        private readonly CancellationTokenSource _cts = new();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = RunAsync(_cts.Token);
            return Task.CompletedTask;
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var input = Console.ReadLine();

                var args = _parser.Parse(input);

                if (args.Length == 0)
                {
                    ThrowHelpers.ThrowInvalidArgument(args);
                }

                var guid = Guid.NewGuid();
                var logger = _factory.CreateLogger($"Commands.Pipeline[{Guid.NewGuid()}]");

                logger.LogTrace("Generating context with ID {}", guid);

                var context = new HostedCommandContext(logger);

                await _manager.TryExecuteAsync(context, args, cancellationToken).ConfigureAwait(false);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            return Task.CompletedTask;
        }
    }
}
