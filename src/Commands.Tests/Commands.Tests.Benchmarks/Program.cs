using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Commands.Tests;

public class BenchmarkCallerContext : AsyncCallerContext
{
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
    static void Main() 
        => BenchmarkRunner.Run<Program>();

    private static readonly ArgumentArray _args = ArgumentArray.From("command");
    private static readonly ComponentManager _components = ComponentManager.From()
        .Type<CreationAnalysisModule>()
        .Component(Command.From(() => { }, "command"))
        .Create();

    [Benchmark] public void CreateArguments() => ArgumentArray.From("command");

    [Benchmark] public void FindCommands() => _components.Find(_args);

    [Benchmark] public void RunCommand() => _components.TryExecute(new BenchmarkCallerContext());

    [Benchmark] public Task RunCommandAsync() => _components.TryExecuteAsync(new BenchmarkCallerContext());

    [Benchmark] public ComponentManager CollectionCreate() => [];

    [Benchmark] public CommandGroup GroupCreate() => new(["name"]);

    [Benchmark] public Command CommandCreate() => new(() => { }, ["name"]);
}