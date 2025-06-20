using Commands.Hosting;
using Commands.Parsing;
using Commands.Samples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/* 
 * This code demonstrates how to set up a simple command listener using Commands.Hosting.
 */

var builder = Host.CreateDefaultBuilder(args);

// First, we configure the host to use factory and provider types according to the settings defined by this builder.
builder.ConfigureComponents(context =>
{
    // We can provide options for command creation and execution here.
    // One of these options is the parsers that are used to convert raw string values into specific types.
    context.ConfigureOptions(options =>
    {
        // In this sample, we add a parser for the Version type, as it is implemented by some commands.
        options.Parsers[typeof(Version)] = new TryParseParser<Version>(Version.TryParse);
    });
});

// After configuring the components, we set up the services for the host. The command listener is responsible for listening to incoming queries and executing commands based on them.
// The service class is a helper class that can be used to encapsulate business logic or shared functionality across commands, within the scope of the command listener's execution context.
builder.ConfigureServices(services =>
{
    services.AddHostedService<CommandListener>();
    services.AddSingleton<VersionManager>();
    services.AddScoped<BasicService>();
});

// We create the host. If there are any issues with the configuration, the host will fail to build, throwing an exception.
var host = builder.Build();

// Finally, we register components with the host. Components can be commands, command groups, or any other type that implements the IComponent interface.
host.UseComponents(tree =>
{
    // By default, the tree is empty. We add all exported types from the current assembly to the component tree, where CommandModule implementations are automatically discovered and registered as components.
    tree.AddRange(typeof(Program).Assembly.GetExportedTypes());
});

// Now that the host is configured and components are registered, we can run the host, and the CommandListener will start listening for incoming commands.
await host.RunAsync();