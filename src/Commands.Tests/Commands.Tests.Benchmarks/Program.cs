using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;

namespace Commands.Tests
{
    [MemoryDiagnoser]
    public class Program
    {
        private readonly ComponentTree _tree;

        public Program()
        {
            var services = new ServiceCollection()
                .AddSingleton(new ComponentTreeBuilder()
                {

                }.Build())
                .BuildServiceProvider();

            _tree = services.GetRequiredService<ComponentTree>();
        }

        static void Main()
        {
            BenchmarkRunner.Run<Program>();
        }

        [Benchmark]
        public void SearchCommand()
        {
            _tree.Find(new ArgumentEnumerator(["scenario"]));
        }

        [Benchmark]
        public void SearchParametered()
        {
            _tree.Find(new ArgumentEnumerator(["scenario-parameterized", "1"]));
        }

        [Benchmark]
        public void SearchNested()
        {
            _tree.Find(new ArgumentEnumerator(["scenario-nested", "scenario-injected"]));
        }

        [Benchmark]
        public async Task RunCommand()
        {
            await _tree!.Execute(new CallerContext(), ["scenario"]);
        }

        [Benchmark]
        public async Task RunParametered()
        {
            await _tree!.Execute(new CallerContext(), ["scenario-parameterized", "1"]);
        }

        [Benchmark]
        public async Task RunNested()
        {
            await _tree!.Execute(new CallerContext(), ["scenario-nested", "scenario-injected"]);
        }

        [Benchmark]
        public async Task RunException()
        {
            await _tree!.Execute(new CallerContext(), ["scenario-exception"]);
        }

        [Benchmark]
        public async Task RunTaskException()
        {
            await _tree!.Execute(new CallerContext(), ["scenario-task-exception"]);
        }

        [Benchmark]
        public async Task RunExceptionThrow()
        {
            await _tree!.Execute(new CallerContext(), ["scenario-exception-throw"]);
        }

        [Benchmark]
        public async Task RunTaskExceptionThrow()
        {
            await _tree!.Execute(new CallerContext(), ["scenario-task-exception-throw"]);
        }

        [Benchmark]
        public async Task RunOperationMutation()
        {
            await _tree!.Execute(new CallerContext(), ["scenario-operation-mutation"]);
        }

        [Benchmark]
        public async Task RunOperationTaskMutation()
        {
            await _tree!.Execute(new CallerContext(), ["scenario-task-operation-mutation"]);
        }

        [Benchmark]
        public async Task RunOperationFormattable()
        {
            await _tree!.Execute(new CallerContext(), ["scenario-operation-formattable"]);
        }

        [Benchmark]
        public async Task RunOperationTaskFormattable()
        {
            await _tree!.Execute(new CallerContext(), ["scenario-task-operation-formattable"]);
        }
    }
}