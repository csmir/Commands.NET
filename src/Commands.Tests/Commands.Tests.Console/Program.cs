using Commands;
using Commands.Tests;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

var tree = ComponentManager.CreateBuilder()
    .ConfigureComponents(configure =>
    {
        configure.AddParser(new CSharpScriptParser());
    })
    .WithTypes(typeof(Program).Assembly.GetExportedTypes())
    .AddResultHandler((c, r, s) => c.Respond(r))
    .Build();

var services = new ServiceCollection()
    .AddSingleton(tree)
    .BuildServiceProvider();

while (true)
{
    var input = AnsiConsole.Prompt(new TextPrompt<string>("Enter a command"));

    using var scope = services.CreateScope();

    var values = ArgumentArray.Read(input);

    var caller = new AsyncCustomCaller();

    await tree.TryExecuteAsync(caller, values, new()
    {
        Services = scope.ServiceProvider
    });
}
