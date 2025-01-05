![Here goes a banner.](https://raw.githubusercontent.com/csmir/Commands.NET/refs/heads/master/img/cnetbanner_lighttrans_outline_bexp.png)

# Commands.NET

![Build Status](https://img.shields.io/github/actions/workflow/status/csmir/CSF.NET/dotnet.yml?branch=master&style=flat)
[![Download](https://img.shields.io/static/v1?style=flat&message=download%20on%20nuget&color=004880&logo=NuGet&logoColor=FFFFFF&label=)](https://nuget.org/packages/Commands.NET)
[![Discord](https://img.shields.io/discord/1092510256384450652?style=flat)](https://discord.gg/T7hCvShAx5)

**Do more, with less. With speed, compatibility and fluent integration in mind.**

Commands.NET aims to improve your experience integrating input from different sources* into the same, concurrent pool and treating them as triggered actions, called commands. It provides a modular and intuitive API for registering and executing commands.

**Sources can range from command-line, console, chatboxes, to social platforms like Discord, Slack, Messenger & much, much more.*

## Documentation

Are you new to Commands.NET, wanting to implement into your applications? 
[Read the quick guide](https://github.com/csmir/Commands.NET/wiki/Quick-Guide) to get started. 

> For a more expanded view, browse the whole [Commands.NET Wiki](https://github.com/csmir/Commands.NET/wiki).

## Usage

### Running a Command

```cs
var command = Command.Create(() => "Hello world!", "greet");

var manager = ComponentManager.Create(command);

manager.TryExecute(new DefaultCallerContext(), args);

// dotnet run greet -> Hello world!
```

### Creating Subcommands

```cs
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

manager.TryExecute(new DefaultCallerContext(), args);

// dotnet run math sum 5 3 -> 8
```

### Creating Modules

```cs
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

manager.TryExecute(new DefaultCallerContext(), args);

// dotnet run help -> Commands: math sum ..., math subtract ..., math multiply ..., math divide ..., help ...
```

## Samples

- [Commands.Samples.Core](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Core)
  - Manage, create and execute commands in a basic console application.
- [Commands.Samples.Console](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Console)
  - Fluent API's, complex execution flow and workflow expansion.
- [Commands.Samples.Hosting](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Hosting)
  - Integrating Commands.NET into the .NET Generic Host infrastructure.
- [Commands.Samples.CLI](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.CLI)
  - Create CLI actions with argument parsing and enhanced functionality through an extension package.

