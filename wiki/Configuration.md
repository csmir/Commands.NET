The configuration of command registration and execution can be overwhelming if you are unfamiliar with the various options exposed. 
This document introduces the various options and elaborates the functionality covered within.

- [Minimal API's](#minimal-apis)
- [Individual Settings](#configuring-the-manager)
- [Pipeline configuration](#configuring-the-pipeline)

## Minimal API's

the `CommandConfiguration` class is designed to adhere to the philosophy of .NET minimal API's. These API's intend to centralize simplicity yet retain full control over the configured values. 
Commands.NET shares that philosophy, and seamlessly integrates with this system.

To start configuring, you can use the `CreateDefaultBuilder` method to create a default configuration.

```cs
var builder = CommandManager.CreateDefaultBuilder();
```

This builder exposes the API's required to manipulate the configuration to your liking.

### Assemblies

Assemblies added to the configuration determine where the framework will look for commands. 

For example, if you add `Assembly.GetExecutingAssembly()`, the framework will look for commands in the assembly where the configuration is defined. 
Any classes marked with `ModuleBase` or an implementation thereof, are considered in this search.

```cs
builder.AddAssembly(Assembly.GetExecutingAssembly());
```

> By default, the entry assembly of the application is added to the configuration.

### Type Converters

TypeConverters, being all implementations of `TypeConverterBase` (as documented [here](../Type-Conversion.md)), need to be registered in the configuration.

```cs
builder.AddTypeConverter<MyCustomTypeConverter>(new MyCustomTypeConverter());
```

The `AddTypeConverter` method also has an overload for implicit conversion creation. This depends on a delegate, which is used to convert the type.

```cs
builder.AddTypeConverter<MyTypeToConvert>((consumer, argument, value, services) => ConvertResult.FromSuccess(new MyTypeToConvert(value)));
```

> When a command implements `IEnumerable<T>` or any implementation in `System.Collections.Generic`, the framework will create an encompassing converter that underlyingly holds your custom converter, for the specific type.

### Result Resolvers

All results, handled or unhandled, are emitted from the manager through a `ResultResolverBase` implementation. When configuring how to handle your results, you also need to add the resolvers to the configuration.

```cs
builder.AddResultResolver<MyCustomResultResolver>(new MyCustomResultResolver());
```

The `AddResultResolver` method also has an overload for implicit result handling. This depends on a delegate, which is used to handle the result.

```cs
builder.AddResultResolver<MyResult>((consumer, result, services) => { });
```

### Commands

Command registration is commonly done by scanning assemblies, but the minimal API also supports defining your own, lightweight commands. 
These commands are based on delegates, which are considered the same as the `MemberInfo` of a module-based command. 

```cs
builder.AddCommand("say", (string value) => $"Sure, I'll say: {value}");
```

Commands that are registered in the minimal API can also consume command state like instance commands. To access the state, you have to set `CommandContext context` as the first parameter in the delegate.

```cs
builder.AddCommand("say", (CommandContext context, string value) => $"Sure, I'll say: {value}");
```

> Just like with module based commands, the signature can contain anything, as long as it is convertible by the registered `TypeConverters`.

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