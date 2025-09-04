using Commands;
using Commands.Hosting;
using Commands.Http;
using Commands.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;


/* 
 * The following code demonstrates how to set up a simple HTTP server using Commands.Hosting and Commands.Http.
 * Code is structured to run in a .NET generic host, which allows for easy integration with other services and components.
 */


var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration((context, config) =>
{
    // Optionally add additional configuration sources here, such as JSON files, environment variables, etc.
    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
});

// First, we are going to configure the host to use HTTP components, which is required for the communication over RESTful APIs. 
// The ConfigureHttpComponents method allows us to set up the HTTP listener, which will listen for incoming requests on a specified prefix (in this case, http://localhost:5000/).
builder.ConfigureHttpComponents(context =>
{
    // This sets up the HTTP listener to listen on the specified prefix. This operation is essential for the HTTP server to function correctly.
    // If no prefix is specified, the host will fail on run as it cannot start the HTTP listener.
    context.WithListener(listener =>
    {
        listener.Prefixes.Add("http://localhost:7000/");
    });

    // Option configuration defines how components created by UseComponents will behave. 
    // This (currently) includes defining parsers and validation of component names.
    context.ConfigureOptions(options =>
    {
        
    });
});

// By setting the minimum log level to Debug, we can see detailed logs about command registration and execution.
builder.ConfigureLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Debug);
});

// After configuring the host, we build it. At this point, all improperly configured settings will throw exceptions, forcing the host to fail if there are any issues.
var host = builder.Build();

// Next, we register components with the host. Components can be commands, command groups, or any other type that implements the IComponent interface.
// In HTTP hosting, commands are typically registered as HTTP endpoints that can be invoked via HTTP requests.
// However, you can also register other types of components that can be used in the command pipeline of the CommandExecutionFactory service available in the host by default.
host.UseComponents(components =>
{
    // This adds all exported types from the current assembly to the component tree, if they are implementations of CommandModule. Other types are skipped.
    components.AddRange(typeof(Program).Assembly.GetExportedTypes());

    // This adds a command that can be invoked via HTTP GET requests under the following url: http://localhost:5000/ping.
    // The command responds with a simple "OK!" message.
    components.Add(new Command([HttpGet] () =>
    {
        return HttpResult.Ok("OK!");
    }, "ping"));

    // This adds a command that can be invoked via HTTP GET requests under the following url: http://localhost:5000/help.
    // The command lists all available commands in the component tree, providing a simple way to discover what commands are available.
    components.Add(new Command([HttpGet] (IComponentProvider components, IContext context) =>
    {
        var commands = components.Components.GetCommands();

        var response = new StringBuilder("Available commands:\n");

        foreach (var command in commands)
            response.AppendLine($"- {command}");

        return HttpResult.Ok(response.ToString());
    }, "help"));
});

// Finally, we run the host. This will start the HTTP server and begin listening for incoming requests.
await host.RunAsync();

// Alternatively, you can manually create a service collection and configure it to use HTTP components.
// This approach is more flexible and allows for more granular control over the services and components used in the application.
// ... Or when you do not intend to implement the whole host, but just want to run the HTTP server on an isolated service collection.
var services = new ServiceCollection()
    .AddHttpComponents(options =>
    {
        options.WithListener(listener =>
        {
           listener.Prefixes.Add("http://localhost:7000/");
        });
    })
    .BuildServiceProvider();

// Retrieve the component provider from the service collection and add the HttpModule to it, rather than calling UseComponents on the host.
services.GetRequiredService<IComponentProvider>()
    .Components.Add<HttpModule>();

// The factory under hosted context implements IHostedService, which means it will start and stop with the host.
// This is not supported in this isolated context, so it is registered as a singleton service and can be started and stopped manually.
await services.GetRequiredService<HttpCommandExecutionFactory>()
    .StartAsync(default);

// Prevent the application from exiting immediately... or when in a larger application, continue running other tasks.
await Task.Delay(Timeout.Infinite);