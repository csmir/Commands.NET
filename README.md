![Here goes a banner.](https://raw.githubusercontent.com/csmir/Commands.NET/refs/heads/master/img/cnetbanner_lighttrans_outline_bexp.png)

# Commands.NET

Consider leaving a ⭐

![Build Status](https://img.shields.io/github/actions/workflow/status/csmir/Commands.NET/dotnet-master.yml?branch=master&style=flat)
[![Download](https://img.shields.io/static/v1?style=flat&message=download%20on%20nuget&color=004880&logo=NuGet&logoColor=FFFFFF&label=)](https://nuget.org/packages/Commands.NET)
[![Discord](https://img.shields.io/discord/1092510256384450652?style=flat)](https://discord.gg/T7hCvShAx5)

**Do more, with less. With speed, compatibility and fluent integration in mind.**

Commands.NET aims to improve your experience integrating input from different sources* into the same, concurrent pool and treating them as triggered actions, called commands. 
It provides a modular and intuitive API for registering and executing commands.

**Sources can range from command-line, console, HTTP, chatboxes, to social platforms like Discord, Slack, Messenger & much, much more.*

## Documentation

**Browse the [wiki](https://github.com/csmir/Commands.NET/wiki) for a full overview of the library.**

## Installation

Commands.NET is available on NuGet. You can install it using the package manager, or the following command:

```bash
dotnet add package Commands.NET
```

Alternatively, adding it to your `.csproj` file:

```xml
<PackageReference Include="Commands.NET" Version="x.x.x" />
```

> [!NOTE]
> `Commands.NET.Hosting` and `Commands.NET.Http` are available as extension packages for respective features,
> allowing you to host commands in the .NET Generic Host and expose them as HTTP endpoints.

## Usage

### Running a Command

A command is a method executed when a specific query is provided. 
By creating a manager to contain said command, you can run it with the provided arguments.

```cs
var provider = new ComponentProvider();

provider.Components.Add(new Command(() => "Hello world!", "greet"));

await provider.Execute(new ConsoleContext(args));

// dotnet run greet -> Hello world!
```

### Creating Command Groups

Command groups are named collections of commands or other command groups. 
Groups allow for subcommand creation, where the group name is a category for its children.

```cs
var mathGroup = new ComponentGroup("math")
{
    new Command((double number, int sumBy)      => number + sumBy, 
        "sum", "add"), 
    new Command((double number, int subtractBy) => number - subtractBy, 
        "subtract", "sub"), 
    new Command((double number, int multiplyBy) => number * multiplyBy, 
        "multiply", "mul"), 
    new Command((double number, int divideBy)   => number / divideBy, 
        "divide", "div")
};

var provider = new ComponentProvider();

provider.Components.Add(mathGroup);

await collection.Execute(new ConsoleContext(args));

// dotnet run math sum 5 3 -> 8
```

### Creating Command Modules

Command modules are classes that can contain commands or nested command modules, which themselves can also contain (sub)commands.

```cs
public class PingModule : CommandModule 
{
    [Name("ping")]
    public string Ping() => "Pong!";

    [Name("pong")]
    public string Pong() => "Ping!";
}

...

var provider = new ComponentProvider();

provider.Components.Add<HelpModule>();
provider.Components.Add(mathGroup);

await provider.Execute(new ConsoleContext(args));

// dotnet run help -> Commands: math sum <...> math subtract <...> math ...
```

### Dependency Injection

Commands.NET is designed to be compatible with dependency injection out of the box, propagating `IServiceProvider` throughout the execution flow.

```cs
var services = new ServiceCollection()
    .AddSingleton<MyService>()
    .AddSingleton<ComponentProvider>();
    .BuildServiceProvider();

var provider = services.GetRequiredService<ComponentProvider>();

provider.Components.Add<HelpModule>();
provider.Components.Add(mathGroup);

await provider.Execute(new ConsoleContext(args), new ExecutionOptions() { Services = services });
```

Modules can be injected directly from the provider. They themselves are considered transient service consumers, being created and disposed of per command execution.

```cs
public class ServicedModule(MyService service) : CommandModule 
{
}
```

### .NET Generic Host

Alongside dependency injection support in the base package, Commands.NET provides an extension package for the .NET Generic Host, allowing you to integrate Commands.NET into your application with ease.

```cs
var host = Host.CreateDefaultBuilder(args)
    .ConfigureComponents(configure => ...)
    .Build();
```

The extension package supports factory-based command execution alongside scope management, allowing you to manage the lifetime of your commands and modules.

## Samples

This repository includes a set of samples with documented code to help you get started with Commands.NET.

- [Manage, create and execute commands in a basic console application.](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Core)
- [Fluent API's, complex execution flow and workflow expansion.](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Console)
- [Use Commands.NET in F# projects.](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.FSharp)
- [Integrating Commands.NET into the .NET Generic Host infrastructure.](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Hosting)
- [Exposing commands as lightweight HTTP endpoints without the heavy lifting.](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Http)
- [Building Native-AOT apps with Commands.NET.](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Aot)

## Benchmarks

Benchmark results are found [here](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Tests/Commands.Tests.Benchmarks/README.md).
