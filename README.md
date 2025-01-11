![Here goes a banner.](https://raw.githubusercontent.com/csmir/Commands.NET/refs/heads/master/img/cnetbanner_lighttrans_outline_bexp.png)

# Commands.NET

Consider leaving a ⭐

![Build Status](https://img.shields.io/github/actions/workflow/status/csmir/CSF.NET/dotnet.yml?branch=master&style=flat)
[![Download](https://img.shields.io/static/v1?style=flat&message=download%20on%20nuget&color=004880&logo=NuGet&logoColor=FFFFFF&label=)](https://nuget.org/packages/Commands.NET)
[![Discord](https://img.shields.io/discord/1092510256384450652?style=flat)](https://discord.gg/T7hCvShAx5)

**Do more, with less. With speed, compatibility and fluent integration in mind.**

Commands.NET aims to improve your experience integrating input from different sources* into the same, concurrent pool and treating them as triggered actions, called commands. 
It provides a modular and intuitive API for registering and executing commands.

> Browse the [wiki](https://github.com/csmir/Commands.NET/wiki) for a full overview of the library.

**Sources can range from command-line, console, chatboxes, to social platforms like Discord, Slack, Messenger & much, much more.*

## Usage

### Running a Command

A command is a method executed when a specific syntax is provided. 
By creating a manager to contain said command, you can run it with the provided arguments.

```cs
using Commands;

var command = Command.Create(() => "Hello world!", "greet");

var manager = ComponentManager.Create(command);

manager.TryExecute(new ConsoleContext(), args);

// dotnet run greet -> Hello world!
```

### Creating Command Groups

Command groups are named collections of commands or other command groups. 
Groups allow for subcommand creation, where the group name is a category for its children.

```cs
using Commands;

var mathCommands = CommandGroup.Create("math");

mathCommands.AddRange(
    Command.Create((double number, int sumBy)      => number + sumBy, 
        "sum", "add"), 
    Command.Create((double number, int subtractBy) => number - subtractBy, 
        "subtract", "sub"), 
    Command.Create((double number, int multiplyBy) => number * multiplyBy, 
        "multiply", "mul"), 
    Command.Create((double number, int divideBy)   => number / divideBy, 
        "divide", "div")
);

var manager = ComponentManager.Create(mathCommands);

manager.TryExecute(new ConsoleContext(), args);

// dotnet run math sum 5 3 -> 8
```

### Creating Command Modules

Command modules are classes that can contain commands or nested command modules, which themselves can also contain (sub)commands.

```cs
using Commands;

public class HelpModule : CommandModule 
{
    [Name("help")]
    public void Help()
    {
        var builder = new StringBuilder()
            .AppendLine("Commands:");

        foreach (var command in Manager!.GetCommands())
            builder.AppendLine(command.GetFullName());

        Respond(builder.ToString());
    }
}

...

var helpCommands = CommandGroup.Create<HelpModule>();

var manager = ComponentManager.Create(mathCommands, helpCommands);

manager.TryExecute(new ConsoleContext(), args);

// dotnet run help -> Commands: math sum <...> math subtract <...> math ...
```

### Using Dependency Injection

Commands.NET is designed to be compatible with dependency injection out of the box, propagating `IServiceProvider` throughout the execution flow.

```cs
using Commands;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .AddSingleton<MyService>()
    .AddSingleton<ComponentManager>(ComponentManager.Create(mathCommands, helpCommands);
    .BuildServiceProvider();

var manager = services.GetRequiredService<ComponentManager>();

manager.TryExecute(new ConsoleContext(), args, new CommandOptions() { Services = services });
```

Modules can be injected directly from the provider. They themselves are considered transient services;

```cs
public class ServicedModule(MyService service) : CommandModule 
{
}
```

## Samples

- [Commands.Samples.Core](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Core)
  - Manage, create and execute commands in a basic console application.
- [Commands.Samples.Console](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Console)
  - Fluent API's, complex execution flow and workflow expansion.
- [Commands.Samples.Hosting](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Hosting)
  - Integrating Commands.NET into the .NET Generic Host infrastructure.

## Benchmarks

| Method          | Mean      | Error    | StdDev   | Gen0   | Allocated |
|---------------- |----------:|---------:|---------:|-------:|----------:|
| FindCommands    |  61.88 ns | 0.657 ns | 0.615 ns | 0.0381 |     240 B |
| RunCommand      | 232.08 ns | 1.505 ns | 1.257 ns | 0.1287 |     808 B |
| RunCommandAsync | 224.92 ns | 1.409 ns | 1.249 ns | 0.1287 |     808 B |

> Ran on 12th Gen Intel(R) Core(TM) i7-1255U

*This chart can be reproduced at any time by running [Commands.Tests.Benchmarks](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Tests/Commands.Tests.Benchmarks).*