using Commands;
using Commands.Samples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(configure =>
    {
        var builder = ComponentTree.CreateBuilder()
            .WithTypes(typeof(Program).Assembly.GetExportedTypes())
            .AddResultHandler<HostedCallerContext>((context, result, services) =>
            {
                switch (result)
                {
                    case InvokeResult invokeResult:
                        context.Respond(result.Exception);
                        break;
                    case SearchResult searchResult:
                        context.Respond("Invalid command.");
                        break;
                }
            });

        configure.AddSingleton((services) => builder.Build());
        configure.AddHostedService<Listener>();
    })
    .ConfigureLogging(configure =>
    {
        configure.AddSimpleConsole();
    })
    .RunConsoleAsync();