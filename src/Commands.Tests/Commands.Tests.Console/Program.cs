using Commands.Core;
using Commands.Helpers;
using Commands.Parsing;
using Commands.Tests;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var collection = new ServiceCollection()
    .ConfigureCommands(configuration =>
    {
        configuration.AsyncApproach = AsyncApproach.Await;
        configuration.OnFailure(async (context, result, services) =>
        {
            await Task.CompletedTask;
            Console.WriteLine(result.Exception);
        });
        configuration.WithAssemblies(Assembly.GetEntryAssembly());
    });

var services = collection.BuildServiceProvider();

var framework = services.GetRequiredService<CommandManager>();

var parser = new StringParser();

while (true)
{
    var input = parser.Parse(Console.ReadLine()!);

    await framework.TryExecuteAsync(new CommandContext(), input);
}