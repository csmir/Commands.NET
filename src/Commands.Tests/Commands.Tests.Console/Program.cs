using Commands.Core;
using Commands.Helpers;
using Commands.Parsing;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var collection = new ServiceCollection()
    .ConfigureCommands(configuration =>
    {
        configuration.WithAssemblies(Assembly.GetEntryAssembly());
    })
    .TryAddResolver((context, result, services) =>
    {
        if (!result.Success())
            Console.WriteLine(result);
    });

var services = collection.BuildServiceProvider();

var framework = services.GetRequiredService<CommandManager>();

var parser = new StringParser();

while (true)
{
    var input = parser.Parse(Console.ReadLine()!);

    await framework.TryExecuteAsync(new CommandContext(), input);
}