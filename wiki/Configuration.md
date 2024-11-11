The configuration of command registration and execution can be overwhelming if you are unfamiliar with the various options exposed. 
This document introduces the various options and elaborates the functionality covered within.

- [Configuring the Manager](#configuring-the-manager)
- [Configuring the Pipeline](#configuring-the-pipeline)

## Configuring the Manager

`ICommandBuilder` serves as the base interface for handling configuration options for the `CommandManager` and how it registers commands/modules. 
It exposes a large amount of options, each useful in different situations.

### Assemblies 

Known assemblies are a core component to how Commands.NET functions, iterating through each type defined per-assembly to register commands and writing them to the `HashSet<ISearchable>` exposed in `CommandManager`. 
This is the collection used to find, match and execute commands.

By default, `Assemblies` is populated by `Assembly.GetEntryAssembly`, which serves as the entry-point executable to run the framework with.

### TypeConverters

In many cases, there is need for custom `TypeConverterBase` implementations to convert types that Commands.NET does not already convert for you. 
These all need to be registered here, in the same way as `Assemblies`.

The `CommandBuilder<T>` exposes methods to write delegate-based conversion patterns, which will automatically be registered and used as a replacement or introduction to the current `TypeConverters`

### ResultResolvers

Results can be handled in a few ways, as documented [[here|Results]]. 

Implementing custom handlers can be done by implementing `ResultResolverBase` or by creating a delegate-based alternative as exposed in `CommandBuilder<T>`.

### Commands

Commands.NET supports more than just module-based commands. 
Delegate-based and static command signatures are also supported out of the box. 

Static signatures written inside modules will be automatically resolved, but can also be written outside of modules and manually registered.

Delegates can be added just like delegate-based `ResultResolver` and `TypeConverter` implementations. Parameters can be self defined, just as they would be in regular commands.

For both static and delegate commands, you can optionally prefix the command parameters with `CommandContext context`. 
This parameter will not be populated by the command arguments, instead being created in-scope and populated with known command information.

### NamingRegex

Commands signatures are often to be written in a specific culture, and casing unique constraint. Configuring this regex determines what that constraint is. 
If a command does not match the constraint, an exception will be thrown to inform the developer.

## Configuring the Pipeline

`CommandOptions` is the type expected by `CommandManager.Execute` to configure how the pipeline is handled.

### Services

Commands.NET internally does not need the `IServiceProvider` to execute commands. It does however, make the process a lot more scaleable. 
Modules in which instanced commands are based, support dependency injection as you would expect it from a `transient` service type. 

For this support to work, you need to pass a scoped or root `IServiceProvider` into `CommandOptions`. 
This provider will pass along the pipeline, containing your registered services for access in every customizable step in the process.

### SkipPreconditions / SkipPostconditions

This configurable option speaks for itself, being able to skip these invidual evaluation steps in the pipeline. 
This functionality can prove useful for administrative users or consoles, which should skip evaluation in the pre- and postconditions.

### CancellationToken

Being an essential part to long-running asynchronous execution, a `CancellationToken` can be passed into the options, for it to be passed around in the pipeline. 
The source of this token can be managed by the developer in order to cancel long-running operations, if necessary.

### AsyncMode

The `AsyncMode` defines how commands are ran. There are two options to choose from:

#### Await 

This is the default setting and tells the pipeline to finish executing before returning control to the caller. 
This ensures that the execution will fully finish executing, whether it failed or not, before allowing another to be executed.

#### Discard

Instead of waiting for the full execution before returning control, the execution will return immediately after the entrypoint is called, slipping thread for the rest of execution. 
When more than one input source is expected to be handled, this is generally the advised method of execution. 

Changing to this setting, the following should be checked for thread-safety:

- Services, specifically those created as singleton or scoped to anything but a single command.
- Implementations of `TypeConverterBase`, `TypeConverterBase{T}`, `ResultResolverBase`, `PreconditionAttribute` and `PostconditionAttribute`.
- Generic collections and objects with shared access.

> To ensure thread safety in any of the above situations, it is important to know what this actually means. 
> For more information, consider reading [this article](https://learn.microsoft.com/en-us/dotnet/standard/threading/managed-threading-best-practices).