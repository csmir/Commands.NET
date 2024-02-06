Dependency Injection is vital to command invocation, managing all types that implement `ModuleBase` and injecting registered services. 
If you do not understand how this works at its core, it is **important** to read the following resources about the terminology and how it's designed in .NET:

> Dependency Injection will be referred to as 'DI' from here forward.

- [DI in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [DI Usages in apps](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-usage)
- [DI Guidelines](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines)

## Setting up Injection

`ModuleBase` has native support for service injection. 
Without the need to set anything up, you can freely pass around services through the primary constructors of any implementations of this base type:

```cs
public class MyModule(MyService service) : ModuleBase
{
    readonly MyService _service = service;
}
```

With access to this functionality, there are a lot of things that can be achieved, simplified and/or expanded upon. Some of this, we will explore in this guide.

### Accessing the CommandManager

The `CommandManager` hosts a lot of information about the execution of commands as a whole. Sometimes, it is needed to access that information in a command. 
Normally, the manager is not passed along into the module, but we can inject it to achieve the same goal.

```cs
public class MyModule(CommandManager manager) : ModuleBase
{
    readonly CommandManager _manager = manager;
    readonly StringParser _parser = new StringParser();

    [Command("search")]
    public void Search([Remainder]string commandQuery) 
    {
        var args = _parser.Parse(commandQuery);
        var searches = _manager.Search(args);

        foreach (var search in searches) 
        {
            Console.WriteLine(search.Command);
        }
    }
}
```

This command allows a user to search for commands defined by the `CommandManager`, returning zero-or-more results based on the input query. 
The manager reveals more than only this, each of it's properties being accessible in this way.

### Accessing other Modules

All types that implement `ModuleBase` are command modules, and usually isolated to being only used to execute a command, before being disposed. 
Though sometimes, a command will need different restrictions to achieve the same goal, calling upon a function in a different module to resolve the rest of execution.

```cs
public class MyModule(MyNestedModule nested) : ModuleBase
{
    readonly MyNestedModule _nested = nested;

    [Group("do")]
    public class MyNestedModule : ModuleBase 
    {
        [Command("something")]
        public void Something(bool oneStep = false) 
        {
            if (oneStep)
                Console.WriteLine("I did something in one step.");
            else
                Console.WriteLine("I did something in more than one step.");
        }
    }

    [Command("something")]
    public void Something() 
    {
        _nested.Something(true);
    }
}
```

While this example may be niche, it shows off some of the DI features introduced by Commands.NET during command execution. 

> It is important to keep in mind that `MyNestedModule` is not prepared for it's own command, it's `Context`, `Services` and `Command` property will be null.

## Command Scopes

Commands.NET is designed to treat a single command input as a *request*. Naturally, viewing it as a request brings with it the necessity to handle them as scopes. 
For this, the pipeline creates a new `IServiceScope` at each call to `TryExecuteX`. When it does so, it takes a specific setting into consideration:

### Scope Approach

`ScopeApproach` serves as a setting to the `CommandConfiguration` that defines how scopes are created. The difference lies in how the disposing pattern is treated. This option is more elaborated in a fitting chapter of the documentation.

> Learn more about `ScopeApproach` and it's effects [[here|Configuration#ScopeApproach]]

### Module Disposing

Modules are part of the scope created for them, so they naturally also implement the disposing pattern. `IDisposable` and `IAsyncDisposable` are made accessible for developers to override in their own modules, allowing to choose to implement either of these patterns manually where necessary.

```cs
public class MyModule : ModuleBase 
{
    public override void Dispose(bool disposing) 
    {

    }
}
```

> This pattern preliminarily implements a defined disposer pattern, where `disposing` represents whether or not to free managed resources, instead of only unmanaged ones.