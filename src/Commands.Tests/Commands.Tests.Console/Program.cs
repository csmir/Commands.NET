using Commands;
using Commands.Tests;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

var tree = ComponentTree.CreateBuilder()
    .ConfigureComponents(configure =>
    {
        configure.AddParser(new CSharpScriptParser());
    })
    .WithRegistrationLogging((action, message) =>
    {
        AnsiConsole.MarkupLineInterpolated($"[grey]action: {action}[/]\n\t{message}");
    })
    .AddResultHandler(async (c, r, s) => await c.Respond(r))
    .Build();

var services = new ServiceCollection()
    .AddSingleton(tree)
    .BuildServiceProvider();

while (true)
{
    var input = AnsiConsole.Prompt(new TextPrompt<string>("Enter a command"));

    using var scope = services.CreateAsyncScope();

    var values = ArgumentParser.ParseKeyValueCollection(input);

    var caller = new CustomCaller()
    {
        ArgumentCount = values.Count()
    };

    await tree.Execute(caller, values, new()
    {
        Services = scope.ServiceProvider
    });

    await scope.DisposeAsync();
}
