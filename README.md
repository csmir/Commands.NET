![Here goes a banner.](https://raw.githubusercontent.com/csmir/Commands.NET/refs/heads/master/img/cnetbanner_lighttrans_outline_bexp.png)

# Commands.NET

![Build Status](https://img.shields.io/github/actions/workflow/status/csmir/CSF.NET/dotnet.yml?branch=master&style=flat)
[![Download](https://img.shields.io/static/v1?style=flat&message=download%20on%20nuget&color=004880&logo=NuGet&logoColor=FFFFFF&label=)](https://nuget.org/packages/Commands.NET)
[![Discord](https://img.shields.io/discord/1092510256384450652?style=flat)](https://discord.gg/T7hCvShAx5)

**Do more, with less. With speed, compatibility and fluent integration in mind.**

Commands.NET aims to improve your experience integrating input from different sources* into the same, concurrent pool and treating them as triggered actions, called commands. It provides a modular and intuitive API for registering and executing commands.

**Sources can range from command-line, console, chatboxes, to social platforms like Discord, Slack, Messenger & much, much more.*

## Getting Started

There are various resources available in order to get started with Commands.NET. Below, you can find samples and directions to the quick guide.

### Documentation

Are you new to Commands.NET, wanting to implement into your applications? 
[Read the quick guide](https://github.com/csmir/Commands.NET/wiki/Quick-Guide) to get started. 

> For a more expanded view, browse the whole [Commands.NET Wiki](https://github.com/csmir/Commands.NET/wiki).

### Examples

Creating commands:

```cs
var command = Command.Create(() => "Hello world!", "greet");

var runner = ComponentManager.Create(command);

runner.Execute("greet");
```

- [Commands.Samples.Core](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Core)
  - Manage, create and execute commands in a basic console application.
- [Commands.Samples.Console](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Console)
  - Fluent API's, complex execution flow and workflow expansion.
- [Commands.Samples.Hosting](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Hosting)
  - Integrating Commands.NET into the .NET Generic Host infrastructure.
- [Commands.Samples.CLI](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.CLI)
  - Create CLI actions with argument parsing and enhanced functionality through an extension package.

## Features

