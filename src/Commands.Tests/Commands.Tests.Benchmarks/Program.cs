using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Commands.Tests;

public class BenchmarkCallerContext(string? input) : AsyncCallerContext
{
    public override Arguments Arguments { get; } = new(input);

    public override Task Respond(object? response)
        => Task.CompletedTask;
}

[Name("group")]
public class CreationAnalysisModule : CommandModule<BenchmarkCallerContext>
{
    [Name("command")]
    public static void Command1() { }

    [Name("command")]
    public static void Command2() { }

    [Name("command")]
    public static void Command3() { }
}

[MemoryDiagnoser]
public class Program
{
    private static readonly Arguments _args = new("command");

    private static readonly ComponentProvider _provider = new(
    [
        new CommandGroup<CreationAnalysisModule>(),
        new Command(() => { }, "command")
    ]);

    static void Main()
        => BenchmarkRunner.Run<Program>();

    [Benchmark]
    public void CreateArguments()
        => _ = new Arguments("command");

    [Benchmark]
    public void FindCommands()
        => _provider.Components.Find(_args);

    [Benchmark]
    public Task RunCommand()
        => _provider.Execute(new BenchmarkCallerContext("command"));

    [Benchmark]
    public Task RunCommandNonBlocking()
        => _provider.Execute(new BenchmarkCallerContext("command"), new ExecutionOptions()
        {
            ExecuteAsynchronously = true,
        });

    [Benchmark]
    public ComponentTree CollectionCreate()
        => [];

    [Benchmark]
    public CommandGroup GroupCreate()
        => new(["name"]);

    [Benchmark]
    public Command CommandCreate()
        => new(() => { }, ["name"]);
}