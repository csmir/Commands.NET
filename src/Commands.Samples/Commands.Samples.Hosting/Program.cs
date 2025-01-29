using Commands;
using Commands.Samples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var properties = ComponentManager.With
            .Types(typeof(Program).Assembly.GetExportedTypes())
            .Handler(ResultHandler.From<HostedCallerContext>((c, e, s) => c.Respond(e)));

        services.AddSingleton((services) => properties.Create());
        services.AddHostedService<Listener>();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddSimpleConsole();
    })
    .RunConsoleAsync();