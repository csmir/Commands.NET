using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Commands.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Commands.Tests;

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
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario"]);
    }

    [Benchmark]
    public async Task RunParametered()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-parameterized", "1"]);
    }

    [Benchmark]
    public async Task RunNested()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-nested", "scenario-injected"]);
    }

    [Benchmark]
    public async Task RunException()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-exception"]);
    }

    [Benchmark]
    public async Task RunTaskException()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-task-exception"]);
    }

    [Benchmark]
    public async Task RunExceptionThrow()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-exception-throw"]);
    }

    [Benchmark]
    public async Task RunTaskExceptionThrow()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-task-exception-throw"]);
    }

    [Benchmark]
    public async Task RunOperationMutation()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-operation-mutation"]);
    }

    [Benchmark]
    public async Task RunOperationTaskMutation()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-task-operation-mutation"]);
    }

    [Benchmark]
    public async Task RunOperationFormattable()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-operation-formattable"]);
    }

    [Benchmark]
    public async Task RunOperationTaskFormattable()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-task-operation-formattable"]);
    }

    public class BenchmarkCaller : ICallerContext
    {
        public void Respond(object? response) { }
    }
}