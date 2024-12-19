![Here goes a banner.](https://raw.githubusercontent.com/csmir/Commands.NET/refs/heads/master/img/cnetbanner_lighttrans_outline_bexp.png)

# Commands.NET

![Build Status](https://img.shields.io/github/actions/workflow/status/csmir/CSF.NET/dotnet.yml?branch=master&style=flat)
[![Download](https://img.shields.io/static/v1?style=flat&message=download%20on%20nuget&color=004880&logo=NuGet&logoColor=FFFFFF&label=)](https://nuget.org/packages/Commands.NET)
[![Discord](https://img.shields.io/discord/1092510256384450652?style=flat)](https://discord.gg/T7hCvShAx5)

**Do more, with less. With fast, compatible and fluent integration in mind.**

Commands.NET aims to improve your experience integrating input from different sources* into the same, concurrent pool and treating them as triggered actions, called commands. It provides a modular, easy to implement pipeline for registering and executing commands.

**Sources can range from command-line, console, chatboxes, to social platforms like Discord, Slack, Messenger & much, much more.*

- [Getting Started](#getting-started)
- [Features](#features)
- [Additional Packages](#additional-packages)

> [!NOTE]
> Commands.NET is CLS-compliant, Native AOT friendly, and built on .NET Standard 2.1

## Getting Started

There are various resources available in order to get started with Commands.NET. Below, you can find samples and directions to the quick guide.

### Documentation

[You can find the quick guide here](https://github.com/csmir/Commands.NET/wiki/Quick-Guide). 
This guide introduces you to the basics of defining modules, commands, and how to run them.

> Or for a more expanded view, browse the whole [Commands.NET Wiki](https://github.com/csmir/Commands.NET/wiki).

### Samples

Samples are available to learn how to implement Commands.NET in your own programs.

- [Commands.Samples.Console](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Console)
  - Implement Commands.NET on a basic console application.
- [Commands.Samples.Hosting](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.Hosting)
  - Implement Commands.NET into .NET Generic Host infrastructure.
- [Commands.Samples.CLI](https://github.com/csmir/Commands.NET/tree/master/src/Commands.Samples/Commands.Samples.CLI)
  - Implement Commands.NET in a CLI app.

## Features

#### Type Conversion

For raw input, automated conversion to fit command signature is supported by `TypeConverter`'s. All `ValueType` types, `Enum` types and various System types such as `TimeSpan` and `DateTime` are automatically parsed by the framework and populate commands as below:

Additionally, all the forementioned types wrapped in implementations of `IEnumerable<T>`, `Array`, or `Nullable<T>` are also supported via the same conversion.

```cs
...
[Name("hello")]
public string World(int worldCount)
{
    return "Hello, world " + worldCount;
}
...
```

- This will automatically parse `int` by using the default `int.TryParse` implementation.

Outside of this, implementing and adding your own `TypeConverterBase` is also supported to handle command signatures with other types.

> See feature [documentation](https://github.com/csmir/Commands.NET/wiki/Type-Conversion) for more.

#### Conditions

Implementing `PreconditionAttribute` creates a new evaluation to add in the set of attributes defined above command definitions. 
When a command is attempted to be executed, it will walk through every precondition present and abort execution if any of them fail.

```cs
...
[Condition]
[Name("hello")]
public Task<string> Command(string world)
{
    return Task.FromResult("Hello, " + world + ". I can only execute when Condition says so!");
}
...
```

In the same way as above, `PostconditionAttribute` can be implemented to add a condition that is evaluated after the command has been executed. 

> See feature [documentation](https://github.com/csmir/Commands.NET/wiki/Conditions) for more.

#### Minimal API's

The API focusses on customizability and configuration above all else, and this is visible in the pre-execution setup. 
It closely matches the design philosophy of .NET Minimal API's, and is designed to be as easy to use as possible.

```cs
var builder = CommandManager.CreateDefaultBuilder();

builder.AddCommand();
builder.AddAssembly();
builder.AddTypeConverter();
builder.AddResultResolver();

var manager = builder.Build();
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

#### Reflection

The framework saves command data in its own reflection types. 
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

Commands.NET functions completely without additional packages, but it certainly improves in functionality when it is extended upon by other packages.
It tries to do as much as it can by itself, but it supports packages when their functionality outweigh self-written implementations. 

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

#### Console

For Console Applications specifically, the existing layer of functionality in Commands.NET can be expanded upon by implementing a prettifier for the console, [Spectre.Console](https://github.com/spectreconsole/spectre.console). 

`Commands.NET.Console` ships with the previously mentioned package, but it does not necessarily expect a particular version. Therefore, it might still be worthwhile to install your own version if you so need to:

```xml
    <PackageReference Include="Spectre.Console" Version="" />
```

*For each of these packages, the minimum version is determined by Commands.NET itself, usually being the latest or equal to the target framework upon which it was released. It is suggested to choose the latest version at time of installation.*
