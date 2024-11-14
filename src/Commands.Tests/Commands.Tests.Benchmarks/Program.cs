using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
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
                .AddSingleton(new ConfigurationBuilder()
                {

                }.Build())
                .BuildServiceProvider();

            _manager = services.GetRequiredService<CommandManager>();
        }

        static void Main()
        {
            BenchmarkRunner.Run<Program>();
        }

        //[Benchmark]
        public void ParseText()
        {
            StringParser.ParseKeyCollection("command");
        }

        [Benchmark]
        public async Task RunCommand()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario"]);
        }

        [Benchmark]
        public async Task RunParametered()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-parameterized", "1"]);
        }

        [Benchmark]
        public async Task RunNested()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-nested", "scenario-injected"]);
        }

        [Benchmark]
        public async Task RunException()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-exception"]);
        }

        [Benchmark]
        public async Task RunTaskException()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-task-exception"]);
        }

        [Benchmark]
        public async Task RunExceptionThrow()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-exception-throw"]);
        }

        [Benchmark]
        public async Task RunTaskExceptionThrow()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-task-exception-throw"]);
        }

        [Benchmark]
        public async Task RunOperationMutation()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-operation-mutation"]);
        }

        [Benchmark]
        public async Task RunOperationTaskMutation()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-task-operation-mutation"]);
        }

        [Benchmark]
        public async Task RunOperationFormattable()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-operation-formattable"]);
        }

        [Benchmark]
        public async Task RunOperationTaskFormattable()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-task-operation-formattable"]);
        }
    }
}