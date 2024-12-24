using Commands;
using Commands.Tests;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

var configuration = new ComponentConfiguration();

var resultHandler = new DelegateResultHandler(async (c, r, s) => await c.Respond(r));

var tree = new ComponentTree(components: configuration.GetComponents(typeof(Program).Assembly), resultHandler);

var services = new ServiceCollection()
    .AddSingleton(tree)
    .BuildServiceProvider();

while (true)
{
    var input = AnsiConsole.Prompt(new TextPrompt<string>("Enter a command").PromptStyle("info"));

    using var scope = services.CreateAsyncScope();

    var values = ArgumentParser.ParseKeyCollection(input);

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
