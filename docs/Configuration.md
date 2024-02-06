The configuration of command registration and execution can be overwhelming if you are unfamiliar with the various options exposed. This chapter introduces the various options and elaborates the functionality covered within.

## The Command Configuration

`CommandConfiguration` serves as the base class for handling configuration options for the `CommandManager` and how it runs the execution pipeline. 
It exposes a fair amount of options, each useful in different situations.

### Assemblies 

Known assemblies are a core component to how Commands.NET functions, iterating through each type defined per-assembly to register commands and writing them to the `IReadOnlySet` exposed in `CommandManager`. This is the collection used to find, match and execute commands.

By default, `Assemblies` is populated by `Assembly.GetEntryAssembly`, which serves as the entry-point executable to run the framework with.

The `CommandConfiguration` exposes an array that is accessible by the following methods:

#### `TryAddAssembly`

This method will attempt to add an `Assembly` to `Assemblies`, returning if the assembly was already added.

#### `AddAssembly`

This method will attempt to add an `Assembly` to `Assemblies`, throwing an exception if the assembly was already added.

#### `WithAssemblies`

This method will replace all current assemblies with `params Assembly[]` as passed into the method. Duplicates are automatically removed through calling `IEnumerable.Union`.

### Type Converters (Converters)

In many cases, there is need for custom `TypeConverter` implementations to convert types that Commands.NET does not already convert for you. These all need to be registered here, in the same way as `Assemblies`. The overloads for adding new type converters is the same as the prior, with no exceptions.

### Result Resolver (ResultResolver)

Results can be handled in an elaborate many ways, as documented [[here|Results]]. This configuration option allows you to set a custom resolver or redefine the base implementation with `ConfigureResultAction`.

### Async Approach (AsyncApproach)

The `AsyncApproach` defines how commands are ran. There are two options aside from `Default`, which is set to `Await`.

#### Await 

This is the default setting and tells the pipeline to finish executing before returning control to the caller. 
This ensures that the execution will fully finish executing, whether it failed or not, before allowing another to be executed.

#### Discard

Instead of waiting for the full execution before returning control, the execution will return immediately after the entrypoint is called, slipping thread for the rest of execution. 
When more than one input source is expected to be handled, this is generally the advised method of execution. 

Changing to this setting, the following should be checked for thread-safety:

- Services, specifically those created as singleton or scoped to anything but a single command.
- Implementations of `TypeConverter`, `TypeConverter{T}` and `PreconditionAttribute`.
- Generic collections and objects with shared access.

> To ensure thread safety in any of the above situations, it is important to know what this actually means. 
> For more information, consider reading [this article](https://learn.microsoft.com/en-us/dotnet/standard/threading/managed-threading-best-practices).

### Scope Approach (ScopeApproach)

The `ScopeApproach` defines how scoping is approached at command pipeline level. The pipeline takes into account all instances 

#### OnlyAsync

This option forces the `IServiceProvider` to use `CreateAsyncScope` to generate new scopes. This structure wraps around `IServiceScope` to carry the ability to handle `IAsyncDisposable` types. This is usually unnecessary, unless if `AsyncApproach` is set to `Discard` but no `IAsyncDisposable` services exist.

#### OnlySync

Here, the option forces the `IServiceProvider` to use `CreateScope`. If no settings are changed in the `CommandConfiguration`, this is the default approach. It is unusual for smaller applications to have scopes that implement `IAsyncDisposable` pattern. Though, if this is the case, this setting must be changed.

#### ByAsyncApproach

Here, `AsyncApproach` determines the value (one of the above).

- When set to `Discard`, it will follow the logic as set in `OnlyAsync`.
- When set to `Await`, it will follow `OnlySync` logic.