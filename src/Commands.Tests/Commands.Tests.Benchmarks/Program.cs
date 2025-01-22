using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

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
            .AddSingleton(ComponentManager.With
                .Type<CreationAnalysisModule>()
                .Component(Command.From(() => { }, "command"))
                .Create())
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
        Command.From(() => { }, "name").Create();
    }

    [Benchmark]
    public void GroupCreate()
    {
        CommandGroup.From("name").Create();
    }

    [Benchmark]
    public void CreateArguments()
    {
        ArgumentArray.From("command");
    }

    #endregion

    #region Pipeline Analysis

    private static readonly ArgumentArray _args = ArgumentArray.From("command");

    [Benchmark]
    public void FindCommands()
    {
        _components.Find(_args);
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