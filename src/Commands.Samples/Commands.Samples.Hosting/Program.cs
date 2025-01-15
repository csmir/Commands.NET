using Commands;
using Commands.Samples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(configure =>
    {
        var properties = ComponentManager.Define()
            .Types(typeof(Program).Assembly.GetExportedTypes())
            .Handler(ResultHandler.Define<HostedCallerContext>()
                .Delegate((c, r, s) => c.Respond(r.Exception?.InnerException != null ? r.Exception.InnerException : r.Exception)));

        configure.AddSingleton((services) => properties.ToManager());
        configure.AddHostedService<Listener>();
    })
    .ConfigureLogging(configure =>
    {
        configure.AddSimpleConsole();
    })
    .RunConsoleAsync();