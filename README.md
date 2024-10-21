![cnetbanner_lighttrans_outline_bexp](https://github.com/csmir/Commands.NET/assets/68127614/af1b5bc4-98b6-48b1-9aaa-4063768816dc)

# Commands.NET

![Build Status](https://img.shields.io/github/actions/workflow/status/csmir/CSF.NET/dotnet.yml?branch=master&style=flat)
[![Download](https://img.shields.io/static/v1?style=flat&message=download%20on%20nuget&color=004880&logo=NuGet&logoColor=FFFFFF&label=)](https://nuget.org/packages/Commands.NET)
[![Discord](https://img.shields.io/discord/1092510256384450652?style=flat)](https://discord.gg/T7hCvShAx5)

Commands.NET is a robust no-nonsense command library that makes creating and processing queries easy for any* interactive platform.
It implements a modular, easy to implement pipeline for registering and executing commands, as well as a wide range of customization options to make development on different platforms as easy as possible.

**This includes console-input, game-chat, social platforms like Discord, Slack, Messenger & much, much more.*

- [Features](#features)
- [Additional Packages](#additional-packages)
- [Getting Started](#getting-started)

## Features

#### Type Conversion

For raw input, automated conversion to fit command signature is supported by `TypeConverter`'s. 
`ValueType`, `Enum` and nullable variant types are automatically parsed by the framework and populate commands as below:

```cs
...
[Command("hello")]
public string Command(string world)
{
    return "Hello, " + world;
}
...
```
- This will automatically parse `int` by using the default `int.TryParse` implementation, and will do the same for the `DateTime`.

Outside of this, implementing and adding your own `TypeConverter`'s is also supported to handle command signatures with normally unsupported types.

> See feature [documentation](https://github.com/csmir/Commands.NET/wiki/Type-Conversion) for more.

#### Preconditions

Implementing `PreconditionAttribute` creates a new evaluation to add in the set of attributes defined above command definitions. 
When a command is attempted to be executed, it will walk through every precondition present and abort execution if any of them fail.

```cs
...
[CustomPrecondition]
[Command("hello")]
public Task<string> Command(string world)
{
    return "Hello, " + world + ". I can only execute when CustomPrecondition says so!";
}
...
```

> See feature [documentation](https://github.com/csmir/Commands.NET/wiki/Preconditions) for more.

#### Extensive Configuration

The API focusses on customizability and configuration above all else, and this is visible in the pre-execution setup.

Registration assemblies, result handling, delegate command definitions and more can be defined in the `CommandBuilder<T>`:

```cs
var builder = CommandManager.CreateBuilder();

...

var manager = builder.Build()
```

> See feature [documentation](https://github.com/csmir/Commands.NET/wiki/Configuration) for more.

#### Customization

While already fully functional out of the box, the framework does not shy away from covering extensive applications with more specific needs, which in turn need more than the base features to function according to its developer's expectations. 

This customization is extended into:

- `ConsumerBase`
- `ModuleBase`
- `TypeConverterBase`
- `ResultResolverBase`
- `PreconditionAttribute`
- `PostconditionAttribute` 

These types can all be inherited and custom ones created for environmental specifics, custom type conversion and more.

> See feature [documentation](https://github.com/csmir/Commands.NET/wiki/Customization) for more.

#### Reflection

The framework saves cached command data in its own reflection types. 
These types, such as `CommandInfo`, `ArgumentInfo` and `ModuleInfo` store informative data about a target, its root module and any submembers.

The reflection data is accessible in various ways, most commonly in scope during type conversion & precondition evaluation.

#### Dependency Injection

Having grown into a vital part of building effective and modern applications, 
Dependency Injection (DI) is an incredibly useful concept to be carried along in the equally modern Commands.NET. 
It integrates this feature deeply into its architecture and supports use across the whole API. 

You can provide an `IServiceProvider` at execution to inject modules with dependencies, in accordance to the conventions `Microsoft.Extensions.DependencyInjection` follows. 

```cs
...
manager.Execute(..., ..., options: new CommandOptions() { Services = ... });
```

## Additional Packages

Commands.NET is not without its own dependencies, but it tries its best to keep all dependencies within a trusted atmosphere, 
using packages only when they outweigh self-written implementations. 
So far, it only depends on packages published by official channels of the .NET ecosystem.

#### Dependency Injection

For applications to function with `Commands.NET`, it is suggested to install DI functionality through Microsoft's publicized package(s):

```xml
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="" />
```

> This package is *not* required. Commands.NET is independent by default, and can function as such.
> This and other required packages can also be installed through the Package Manager, .NET CLI or from within your IDE.

#### Hosting

Carrying out further support within the .NET ecosystem, Commands.NET also introduces a hosting package for deploying apps with the .NET generic and application host. 

For applications to function with `Commands.NET.Hosting`, it is necessary to install the hosting package that it also implements, also publicized by Microsoft itself:

```xml
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="" />
```

> The hosting extensions package publicized by Microsoft implements the packages necessary for the core component of Commands.NET, and does not expect to have its dependencies implemented alongside it.

*For each of these packages, the minimum version is determined by Commands.NET itself, usually being the latest or equal to the target framework upon which it was released. It is suggested to choose the latest version at time of installation.*

## Getting Started

There are various resources available in order to get started with Commands.NET. Below, you can find samples and directions to the quick guide.

#### Quick Guide

You can find the quick guide [here](https://github.com/csmir/Commands.NET/wiki/Quick-Guide). 
This guide introduces you to the basics of defining modules, commands, and how to run them.

#### Samples

Samples are available to learn how to implement Commands.NET in your own programs.

- [Commands.Samples.Console](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Console)
  - Shows how to implement Commands.NET on a basic console application.
- [Commands.Samples.Hosting](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Console)
  - Shows how to implement Commands.NET on a hosted console application.
