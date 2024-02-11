using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Commands.Helpers;
using Commands.Parsing;
using Microsoft.Extensions.DependencyInjection;

namespace Commands.Tests
{
    [MemoryDiagnoser]
    public class Program
    {
        private readonly CommandManager _manager;

        public Program()
        {
            var services = new ServiceCollection()
                .ConfigureCommands()
                .BuildServiceProvider();

            _manager = services.GetRequiredService<CommandManager>();
        }

        static void Main()
            => BenchmarkRunner.Run<Program>();

        [Benchmark]
        public void ParseText()
        {
            StringParser.Parse("command");
        }

        [Benchmark]
        public async Task RunCommand()
        {
            await _manager!.TryExecuteAsync(new ConsumerBase(), ["base-test"]);
        }

        [Benchmark]
        public async Task RunParametered()
        {
            await _manager!.TryExecuteAsync(new ConsumerBase(), ["param-test", "1"]);
        }

        [Benchmark]
        public async Task RunNested()
        {
            await _manager!.TryExecuteAsync(new ConsumerBase(), ["nested", "test"]);
        }
    }
}