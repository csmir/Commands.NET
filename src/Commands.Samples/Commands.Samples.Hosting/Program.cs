using Commands;
using Commands.Samples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(configure =>
    {
        var properties = ComponentManager.With
            .Types(typeof(Program).Assembly.GetExportedTypes())
            .Handler(ResultHandler.From<HostedCallerContext>((c, r, s) => c.Respond(r.Unfold())));

        configure.AddSingleton((services) => properties.Create());
        configure.AddHostedService<Listener>();
    })
    .ConfigureLogging(configure =>
    {
        configure.AddSimpleConsole();
    })
    .RunConsoleAsync();