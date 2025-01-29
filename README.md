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

var command = Command.From(() => "Hello world!", "greet");

var collection = ComponentCollection.With.Component(command).Create();

await collection.Execute(new ConsoleContext(args));

// dotnet run greet -> Hello world!
```

### Creating Command Groups

Command groups are named collections of commands or other command groups. 
Groups allow for subcommand creation, where the group name is a category for its children.

```cs
using Commands;

var mathCommands = CommandGroup.From("math")
    .Components(
        Command.From((double number, int sumBy)      => number + sumBy, 
            "sum", "add"), 
        Command.From((double number, int subtractBy) => number - subtractBy, 
            "subtract", "sub"), 
        Command.From((double number, int multiplyBy) => number * multiplyBy, 
            "multiply", "mul"), 
        Command.From((double number, int divideBy)   => number / divideBy, 
            "divide", "div")
    );

var collection = ComponentCollection.With.Components(mathCommands).Create();

await collection.Execute(new ConsoleContext(args));

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

var collection = ComponentCollection.With.Component(mathCommands).Type<HelpModule>().Create();

await collection.Execute(new ConsoleContext(args));

// dotnet run help -> Commands: math sum <...> math subtract <...> math ...
```

### Using Dependency Injection

Commands.NET is designed to be compatible with dependency injection out of the box, propagating `IServiceProvider` throughout the execution flow.

```cs
using Commands;
using Microsoft.Extensions.DependencyInjection;

...

var services = new ServiceCollection()
    .AddSingleton<MyService>()
    .AddSingleton<ComponentCollection>(ComponentCollection.With.Component(mathCommands).Type<HelpModule>().Create());
    .BuildServiceProvider();

var collection = services.GetRequiredService<ComponentCollection>();

await collection.Execute(new ConsoleContext(args), new CommandOptions() { Services = services });
```

Modules can be injected directly from the provider. They themselves are considered transient, being created and disposed of per command execution.

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

Benchmark results are found [here](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Tests/Commands.Tests.Benchmarks/README.md).