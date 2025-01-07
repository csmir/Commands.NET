The builder API's are designed to be flexible and extensible fluent interfaces that allow for easy configuration of the library.

This article will introduce the builder API's and how to use them.

- [Component Configuration](#command-configuration)
- [Component Manager](#component-manager)
- [Commands](#commands)
- [Command Groups](#command-groups)

## Component Configuration

`IConfigurationBuilder` is an interactible interface to customize how library components are created. 
It builds into instances of `ComponentConfiguration`, which is a necessary type to extend component functionality where needed. 

To use this builder, you must first create an instance of it:
```cs
var builder = ComponentConfiguration.CreateBuilder();
```

### Adding Parsers

Parsers are used to convert string input into a specific type. For more information on parsers, see [[Type Parsing|Type-Parsing]].

To add a parser to the configuration, use the `AddParser` method:
```cs
builder.AddParser(new SystemTypeParser(true));
```

Or, to add multiple parsers at once, use the `AddParsers` method:
```cs
builder.AddParsers(new SystemTypeParser(true), new CustomParser());
```

### Configuration Properties

A host-builder inspired API is implemented to configure components with user-defined configuration. 

One of these properties is `NameValidationExpression`, a regular expression that defines the rules for valid command names.

To set this property, set the `WithNameValidationExpression` string value in the property dictionary:
```cs
builder.Properties["NameValidationExpression"] = "^[a-zA-Z0-9_]+$";
```

### Building the Configuration

After you have configured the builder, you can build it into a `ComponentConfiguration` instance:
```cs
var configuration = builder.Build();
```

## Component Manager

`IManagerBuilder` is an interactible interface to customize how the `ComponentManager` is created. 
It wraps around `IConfigurationBuilder` to configure the manager with the desired components, instead of doing it manually.

To use this builder, you must first create an instance of it:
```cs
var builder = ComponentManager.CreateBuilder();
```

### Customizing Configuration

`Configuration` is a property on the builder that allows you to set the configuration for the manager. 
This property is set to a new instance of `ComponentConfigurationBuilder` by default.

By calling the `ConfigureComponents` method, or immediately accessing the `Configuration` property, you can set the configuration:
```cs
builder.ConfigureComponents(config => config.AddParser(new SystemTypeParser(true)));
```

### Result Handling

The builder has API's to add result handlers to the manager. 

To add a result handler, use the `AddResultHandler` method:
```cs
builder.AddResultHandler(new CustomResultHandler());
```

> [!NOTE]
> By defining a different `TCaller` for each handler, you can customize when the handler is called depending on the provided `ICallerContext` during execution.

### Adding Module Types

Modules can be defined throughout the whole assembly, or even multiple assemblies. The builder provides overloads for adding these modules as their types. 
If a type is not a module type, it is ignored, so no filtering needs to be done by the end-user.

To add types, use the `AddType`, `AddTypes` or `WithTypes` methods:
```cs
builder.AddType(typeof(MyModule));
builder.AddTypes(typeof(MyModule), typeof(MyOtherModule));
builder.WithTypes(Assembly.GetExecutingAssembly().GetExportedTypes());
```

### Adding Dynamic Components

`IComponentBuilder` implementations can be added to the builder by a number of methods. Configuring the component is done by any of the below builders.

To add components, use the `AddCommand` or `AddCommandGroup` methods:

```cs
builder.AddCommand(new CommandBuilder());
builder.AddCommandGroup(new CommandGroupBuilder());
```

### Building the Manager

After you have configured the builder, you can build it into a `ComponentManager` instance:
```cs
var manager = builder.Build();
```

## Commands

`CommandBuilder` is an implementation of `IComponentBuilder` that allows for easy configuration of commands.

To use this builder, you must first create an instance of it:
```cs
var builder = new CommandBuilder();
```

### Setting a Handler

The `Handler` property is a delegate that is called when the command is executed.

To set the handler, use the `WithHandler` method:
```cs
builder.WithHandler((int arg1, int arg2) => { });
```

### Setting a Name

A command is required to have at least 1 name. The `Names` property is a collection of names that the command can be invoked with.

To set the names, use the `AddName` or `WithNames` method:
```cs
builder.WithNames("command", "cmd");
builder.AddName("command");
```

### Building the Command

After you have configured the builder, you can build it into a `Command` instance:
```cs
var command = builder.Build();
```

> [!NOTE]
> `IComponentBuilder` build overloads accept a `ComponentConfiguration` instance to configure the component with a custom configuration.
> The `IManagerBuilder` accepts `IComponentBuilder` instances that are not yet built, using its own defined `IConfigurationBuilder` to do so, when its own `Build` method is called.

## Command Groups

`CommandGroupBuilder` is an implementation of `IComponentBuilder` that allows for easy configuration of command groups.

To use this builder, you must first create an instance of it:
```cs
var builder = new CommandGroupBuilder();
```

### Adding Commands

Commands can be added to the group by using the `AddCommand` method.

To add a command, use the `AddCommand` method:
```cs
builder.AddCommand(new CommandBuilder());
```

### Setting a Name

Dissimilar to commands, groups are not required to have a name, but it can have one. 
The `Names` property is a collection of names that the group can be discovered with.

To set the names, use the `AddName` or `WithNames` method:
```cs
builder.WithNames("group", "grp");
builder.AddName("group");
```

> When a group is built and added to anything but a `ComponentManager`, it requires a name to be discovered. 
> Otherwise, it will be considered top-level, and its commands are added to the manager directly.

### Building the Group

After you have configured the builder, you can build it into a `CommandGroup` instance:
```cs
var group = builder.Build();
```