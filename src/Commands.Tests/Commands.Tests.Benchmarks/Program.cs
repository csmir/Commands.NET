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
    private readonly IComponentTree _components;

    public Program()
    {
        var services = new ServiceCollection()
            .AddSingleton(ComponentTree.CreateBuilder()
                .WithTypes(typeof(Program).Assembly.GetTypes())
                .AddCommand("command", () => { })
                .Build())
            .BuildServiceProvider();

        _components = services.GetRequiredService<IComponentTree>();
    }

    static void Main()
    {
        BenchmarkRunner.Run<Program>();
    }

    #region Object Creation Analysis

    [Benchmark]
    public void CommandCreate()
    {
        Command.Create(() => { }, "name");
    }

    [Benchmark]
    public void GroupCreate()
    {
        var group = CommandGroup.Create("name");

        group.Add(Command.Create(() => { }, "name"));
    }

    [Benchmark]
    public void TypeGroupCreate()
    {
        CommandGroup.Create<CreationAnalysisModule>();
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
        _components.Execute(new BenchmarkCaller(), ["command"]);
    }

    [Benchmark]
    public Task RunCommandAsync()
    {
        return _components.ExecuteAsync(new BenchmarkCaller(), ["command"]);
    }

    #endregion

    public class BenchmarkCaller : ICallerContext
    {
        public void Respond(object? response) { }
    }
}