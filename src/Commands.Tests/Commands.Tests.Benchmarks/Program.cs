using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Commands.Tests;

public class BenchmarkCallerContext(string? input) : AsyncCallerContext
{
    public override ArgumentDictionary Arguments { get; } = ArgumentDictionary.From(input);

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
    private static readonly ArgumentDictionary _args = ArgumentDictionary.From("command");
    private static readonly ComponentManager _components = ComponentManager.From()
        .Type<CreationAnalysisModule>()
        .Component(Command.From(() => { }, "command"))
        .Create();

    static void Main() 
        => BenchmarkRunner.Run<Program>();

    [Benchmark] 
    public void CreateArguments() 
        => ArgumentDictionary.From("command");

    [Benchmark] 
    public void FindCommands() 
        => _components.Find(_args);

    [Benchmark] 
    public Task RunCommand() 
        => _components.ExecuteBlocking(new BenchmarkCallerContext("command"));

    [Benchmark] 
    public Task RunCommandNonBlocking() 
        => _components.Execute(new BenchmarkCallerContext("command"));

    [Benchmark] 
    public ComponentManager CollectionCreate() 
        => [];

    [Benchmark] 
    public CommandGroup GroupCreate() 
        => new(["name"]);

    [Benchmark] 
    public Command CommandCreate() 
        => new(() => { }, ["name"]);
}