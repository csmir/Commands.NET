using Commands;
using Commands.Builders;
using Commands.Tests;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

ComponentConfigurationBuilder.Default.AddParser(new CSharpScriptParser());

var resultHandler = new DelegateResultHandler(async (c, r, s) => await c.Respond(r));

var tree = new ComponentTree(ComponentConfiguration.Default.GetComponents(typeof(Program).Assembly), resultHandler);

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
