using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;

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
    private readonly ComponentManager _components;

    public Program()
    {
        var services = new ServiceCollection()
            .AddSingleton(ComponentManager.From()
                .Type<CreationAnalysisModule>()
                .Component(Command.From(() => { }, "command"))
                .ToManager())
            .BuildServiceProvider();

        _components = services.GetRequiredService<ComponentManager>();
    }

    static void Main()
    {
        BenchmarkRunner.Run<Program>();
    }

    #region Object Creation Analysis

    [Benchmark]
    public void CommandCreate()
    {
        //Command.Create(() => { }, "name");
    }

    [Benchmark]
    public void GroupCreate()
    {
        //var group = CommandGroup.Create("name");

        //group.Add(Command.Create(() => { }, "name"));
    }

    [Benchmark]
    public void TypeGroupCreate()
    {
        //CommandGroup.Create<CreationAnalysisModule>();
    }

    [Benchmark]
    public void CreateArguments()
    {
        ArgumentArray.Read("command");
    }

    #endregion

    #region Pipeline Analysis

    [Benchmark]
    public void FindCommands()
    {
        _components.Find(new ArgumentArray(["command"]));
    }

    [Benchmark]
    public void RunCommand()
    {
        _components.TryExecute(new BenchmarkCallerContext(), "command");
    }

    [Benchmark]
    public Task RunCommandAsync()
    {
        return _components.TryExecuteAsync(new BenchmarkCallerContext(), "command");
    }

    #endregion
}