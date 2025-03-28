using Commands;
using Commands.Parsing;
using Commands.Samples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // We configure a configuration for our components. Most of the time, we do this to define custom parsers.
        // TypeParser implementations allow command arguments to be parsed into the expected format and type.
        services.AddSingleton((services) =>
        {
            var config = new ComponentConfiguration();

            config.Parsers[typeof(Version)] = new TryParseParser<Version>(Version.TryParse);

            return config;
        });

        // We configure a component manager to manage our components.
        // This manager requires a configuration and a set of handlers to process the results of command execution.
        services.AddSingleton((services) =>
        {
            var config = services.GetRequiredService<ComponentConfiguration>();
            var logger = services.GetRequiredService<ILogger<Program>>();

            var handler = new DelegateResultHandler<HostedCallerContext>((c, e, s) => c.Respond(e));

            var manager = new ComponentCollection(config, [handler]);

            logger.LogInformation("Component manager created.");

            // We add all exported types from the current assembly to the manager.
            var componentCount = manager.AddRange(typeof(Program).Assembly.GetExportedTypes());

            logger.LogInformation("Added {} components to the manager.", componentCount);

            return manager;
        });

        services.AddHostedService<Listener>();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddSimpleConsole();
    })
    .RunConsoleAsync();