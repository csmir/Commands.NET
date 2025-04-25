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

The package extends the `IHostBuilder` interface with the `ConfigureComponents` method, which can be used to configure execution, 
discovery and configuration of any commands in the assembly or provided types.

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

The default implementation of this method will scan the assembly for any classes that inherit from `CommandModule` and add them to the host. 
Additionally, it defines `CommandExecutionFactory` as the standard factory.

The `ConfigureComponents` method can also be used to configure the host with a custom configuration action. 
This instance can be used to configure the collection of commands, including the build configuration, adding components and adding result handlers:

```csharp
using Commands;
using Commands.Hosting;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
	.ConfigureComponents(configureComponents => 
	{
		configureComponents.WithConfiguration(configureBuild =>
		{
			configureBuild.AddParser(new TryParseParser<Version>(Version.TryParse));
		});
		configureComponents.AddResultHandler<ConsoleCallerContext>((c, e, s) => c.Respond(e));
		configureComponents.AddComponentTypes(typeof(Program).Assembly.GetExportedTypes());
	})
	.Build();
```

## Factory Execution

The `ConfigureComponents` method implicitly adds a number of services that are used to enable scoped command execution. The following services are added:

| Service                    | Lifetime  | Description																												|
| :------------------------- | :-------- | :-----------																												|
| `IExecutionProvider`		 | Singleton | Contains and discovers executable commands based on the factory-provided information.									|
| `ComponentConfiguration`   | Singleton | Used to configure the component collection.																				|
| `IExecutionFactory`		 | Singleton | Used to create instances of `IExecutionContext` for each command execution.												|
| `IExecutionContext`		 | Scoped    | Represents the lifetime of a command, containing the caller and possible cancellation.									|
| `ICallerContextAccessor<>` | Transient | Used to access the caller of the command. This transient service requests the `IExecutionContext` to retrieve the caller.|

> [!TIP]
> Service lifetimes determine how the service should be treated and in what context it is available. 
> In order to understand what implications this has on your codebase, 
> it is recommended to have [a good understanding of what lifetimes mean](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes). 

In order to execute commands through these interfaces, 
inject the `IExecutionFactory` into your class and call `CreateExecution` with the execution data you wish to run against.

## Command Listener

An example usage for the `IExecutionFactory` lies in a generic host console application. 
A simple command listener can be created by implementing the `BackgroundService` class and using the `IExecutionFactory` 
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
            var context = new ConsoleCallerContext(Console.ReadLine());

            await factory.CreateExecution(context);
        }
    }
}
```

This class can be registered with the host using the `ConfigureServices` method, and will be executed when the host is started.
It is of importance that when adding this service, `.AddHostedService` is used, as this will ensure that the service is started and stopped correctly.