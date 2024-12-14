using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;

namespace Commands.Tests
{
    [MemoryDiagnoser]
    public class Program
    {
        private readonly CommandTree _manager;

        public Program()
        {
            var services = new ServiceCollection()
                .AddSingleton(new CommandTreeBuilder()
                {

                }.Build())
                .BuildServiceProvider();

            _manager = services.GetRequiredService<CommandTree>();
        }

        static void Main()
        {
            BenchmarkRunner.Run<Program>();
        }

        //[Benchmark]
        public void ParseText()
        {
            CommandParser.ParseKeyCollection("command");
        }

        [Benchmark]
        public void SearchCommand()
        {
            _manager.Find(new ArgumentEnumerator(["scenario"]));
        }

        [Benchmark]
        public void SearchParametered()
        {
            _manager.Find(new ArgumentEnumerator(["scenario-parameterized", "1"]));
        }

        [Benchmark]
        public void SearchNested()
        {
            _manager.Find(new ArgumentEnumerator(["scenario-nested", "scenario-injected"]));
        }

        //[Benchmark]
        public async Task RunCommand()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario"]);
        }

        //[Benchmark]
        public async Task RunParametered()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-parameterized", "1"]);
        }

        //[Benchmark]
        public async Task RunNested()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-nested", "scenario-injected"]);
        }

        //[Benchmark]
        public async Task RunException()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-exception"]);
        }

        //[Benchmark]
        public async Task RunTaskException()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-task-exception"]);
        }

        //[Benchmark]
        public async Task RunExceptionThrow()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-exception-throw"]);
        }

        //[Benchmark]
        public async Task RunTaskExceptionThrow()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-task-exception-throw"]);
        }

        //[Benchmark]
        public async Task RunOperationMutation()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-operation-mutation"]);
        }

        //[Benchmark]
        public async Task RunOperationTaskMutation()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-task-operation-mutation"]);
        }

        //[Benchmark]
        public async Task RunOperationFormattable()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-operation-formattable"]);
        }

        //[Benchmark]
        public async Task RunOperationTaskFormattable()
        {
            await _manager!.Execute(new ConsumerBase(), ["scenario-task-operation-formattable"]);
        }
    }
}