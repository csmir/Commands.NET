using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Commands.Core;
using Commands.Helpers;
using Commands.Parsing;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS8321

var services = new ServiceCollection()
    .ConfigureCommands()
    .AddLogging()
    .BuildServiceProvider();

var manager = services.GetRequiredService<CommandManager>();

BenchmarkRunner.Run<Program>();

[Benchmark]
void ParseText()
{
    StringParser.Parse("command");
}

[Benchmark]
void RunCommand()
{
    manager!.TryExecute(new ConsumerBase(), "base-test");
}

[Benchmark]
void RunParametered()
{
    manager!.TryExecute(new ConsumerBase(), "param-test", "1");
}

[Benchmark]
void RunNested()
{
    manager!.TryExecute(new ConsumerBase(), "nested", "test");
}