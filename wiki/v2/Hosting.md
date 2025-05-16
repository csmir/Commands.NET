The .NET generic host is a popular solution implemented by commonly used tools shipped by Microsoft such as EntityFramework, ASP.NET, Blazor, Azure and more.
Commands.NET extends the generic host with its own package in order to provide a seamless integration into your application.

This article covers all available tools the library provides to host commands, including the .NET generic host and the service pattern.

- [Package Download](#download)
- [Configure the Host](#configure-the-host)
- [Factory Execution](#factory-execution)
- [Command Listener](#command-listener)

## Download

The hosting package is available on NuGet. You can install it using the package manager, or the following command:

```bash
dotnet add package Commands.NET.Hosting
```

Alternatively, adding it to your `.csproj` file:

```xml
<PackageReference Include="Commands.NET.Hosting" Version="x.x.x" />
```

## Configure the Host

The package extends the `IHostBuilder` interface with the `ConfigureComponents` method, which can be used to configure discovery and configuration of any commands in the assembly or provided types.

> [!IMPORTANT]
> The ConfigureComponents method also accepts a `TFactory` type, 
> which is the implementation of `CommandExecutionFactory` to be used by the host, as explained in the [Factory Execution](#factory-execution) section.

```csharp
using Commands.Hosting;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
	.ConfigureComponents()
	.Build();
```

The `ConfigureComponents` method can also be used to configure the host with a custom configuration action. 
This instance can be used to configure the collection of commands, including the build configuration and adding result handlers:

```csharp
using Commands;
using Commands.Hosting;
using Commands.Parsing;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
	.ConfigureComponents(components => 
	{
        components.Configure(options =>
        {
            options.Parsers[typeof(Version)] = new TryParseParser<Version>(Version.TryParse);
        });

        components.AddResultHandler(new HandlerDelegate<ConsoleContext>((c, e, s) => c.Respond(e)));
	})
	.Build();

host.UseComponents(components => 
{
    tree.AddRange(typeof(Program).Assembly.GetExportedTypes());
});

host.Run();
```

After the host has been built, it is of importance to call `UseComponents` to ensure that the components are registered with the host. 
Doing so after the host has been configured ensures that the components use the correct configuration.

## Factory Execution

The `ConfigureComponents` method implicitly adds a number of services that are used to enable scoped command execution. The following services are added:

| Service                    | Lifetime  | Description																												|
| :------------------------- | :-------- | :-----------																												|
| `ICommandExecutionFactory` | Singleton | Used to create instances of `IExecutionContext` for each command execution, and managing the scope lifetime.				|
| `IComponentProvider`		 | Singleton | Contains and discovers executable commands based on the factory-provided information.									|
| `IDependencyResolver`	     | Singleton | Used to manage service injection for modules and statically or delegate defined commands.								|
| `IExecutionScope`	         | Scoped    | Represents the lifetime of a command, containing the caller and possible cancellation.									|
| `IContextAccessor<out T>`  | Transient | Used to access the caller of the command. This transient service requests the `IExecutionContext` to retrieve T.			|

> [!TIP]
> Service lifetimes determine how the service should be treated and in what context it is available. 
> In order to understand what implications this has on your codebase, 
> it is recommended to have [a good understanding of what lifetimes mean](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes). 

In order to execute commands through these interfaces, 
inject the `ICommandExecutionFactory` into your class and call `StartExecution` with the execution data you wish to run against.

## Command Listener

An example usage for the `ICommandExecutionFactory` lies in a generic host console application. 
A simple command listener can be created by implementing the `BackgroundService` class and using the `ICommandExecutionFactory` 
to create a new execution context for each command:

```cs
using Commands;
using Commands.Hosting;
using Microsoft.Extensions.Hosting;

public sealed class CommandListener(IExecutionFactory factory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var context = new ConsoleContext(Console.ReadLine());

            await factory.StartExecution(context);
        }
    }
}
```

This class can be registered with the host using the `ConfigureServices` method, and will be executed when the host is started.
It is of importance that when adding this service, `.AddHostedService` is used, as this will ensure that the service is started and stopped correctly:

```cs
...

hostBuilder.ConfigureServices(services =>
{
    services.AddHostedService<CommandListener>();
});

...
```