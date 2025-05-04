using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Commands.Tests;

public class BenchmarkCallerContext(string? input) : AsyncCallerContext
{
    public override ArgumentDictionary Arguments { get; } = ArgumentDictionary.FromString(input);

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
    private static readonly ArgumentDictionary _args = ArgumentDictionary.FromString("command");
    private static readonly ComponentProvider _components = ComponentProvider.From()
        .AddComponentType<CreationAnalysisModule>()
        .AddComponent(Command.From(() => { }, "command"))
        .ToProvider();

    static void Main()
        => BenchmarkRunner.Run<Program>();

    [Benchmark]
    public void CreateArguments()
        => ArgumentDictionary.FromString("command");

    [Benchmark]
    public void FindCommands()
        => _components.Find(_args);

    [Benchmark]
    public Task RunCommand()
        => _components.Execute(new BenchmarkCallerContext("command"));

    [Benchmark]
    public Task RunCommandNonBlocking()
        => _components.Execute(new BenchmarkCallerContext("command"), new CommandOptions()
        {
            ExecuteAsynchronously = true,
        });

    [Benchmark]
    public ComponentProvider CollectionCreate()
        => [];

    [Benchmark]
    public CommandGroup GroupCreate()
        => new(["name"]);

    [Benchmark]
    public Command CommandCreate()
        => new(() => { }, ["name"]);
}