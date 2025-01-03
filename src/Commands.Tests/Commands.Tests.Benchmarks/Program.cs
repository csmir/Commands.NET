using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;

namespace Commands.Tests;

[MemoryDiagnoser]
public class Program
{
    private readonly ComponentTree _tree;

    public Program()
    {
        var services = new ServiceCollection()
            .AddSingleton(ComponentTree.CreateBuilder().WithTypes(typeof(Program).Assembly.GetTypes()).Build())
            .BuildServiceProvider();

        _tree = (services.GetRequiredService<IComponentTree>() as ComponentTree)!;
    }

    static void Main()
    {
        BenchmarkRunner.Run<Program>();
    }

    //[Benchmark]
    public void CreateArray()
    {
        new ArgumentArray(["scenario"]);
    }

    [Benchmark]
    public void SearchCommand()
    {
        _tree.Find(new ArgumentArray(["scenario"]));
    }

    //[Benchmark]
    public void SearchParametered()
    {
        _tree.Find(new ArgumentArray(["scenario-parameterized", "1"]));
    }

    //[Benchmark]
    public void SearchNested()
    {
        _tree.Find(new ArgumentArray(["scenario-nested", "scenario-injected"]));
    }

    [Benchmark]
    public async Task RunCommand()
    {
        await _tree.ExecuteAsync(new BenchmarkCaller(), ["scenario"]);
    }

    //[Benchmark]
    public async Task RunParametered()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-parameterized", "1"]);
    }

    //[Benchmark]
    public async Task RunNested()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-nested", "scenario-injected"]);
    }

    //[Benchmark]
    public async Task RunException()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-exception"]);
    }

    //[Benchmark]
    public async Task RunTaskException()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-task-exception"]);
    }

    //[Benchmark]
    public async Task RunExceptionThrow()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-exception-throw"]);
    }

    //[Benchmark]
    public async Task RunTaskExceptionThrow()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-task-exception-throw"]);
    }

    //[Benchmark]
    public async Task RunOperationMutation()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-operation-mutation"]);
    }

    //[Benchmark]
    public async Task RunOperationTaskMutation()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-task-operation-mutation"]);
    }

    //[Benchmark]
    public async Task RunOperationFormattable()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-operation-formattable"]);
    }

    //[Benchmark]
    public async Task RunOperationTaskFormattable()
    {
        await _tree!.ExecuteAsync(new BenchmarkCaller(), ["scenario-task-operation-formattable"]);
    }

    public class BenchmarkCaller : ICallerContext
    {
        public void Respond(object? response) { }
    }
}