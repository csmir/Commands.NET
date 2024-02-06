// This sample implements the same structure as https://github.com/csmir/Commands.NET/wiki/Quick-Guide.
// It also implements examples for Precondition and TypeConverter documentation.

using Commands.Core;
using Commands.Helpers;
using Commands.Parsing;
using Commands.Samples;

using Microsoft.Extensions.DependencyInjection;

var collection = new ServiceCollection()
    .ConfigureCommands(configuration =>
    {
        configuration.ConfigureFailureAction((context, result, services) =>
        {
            Console.WriteLine(result.Exception);    

            return Task.CompletedTask;
        });
        configuration.AddTypeReader(new ReflectionTypeConverter(caseIgnore: true));
    });

var services = collection.BuildServiceProvider();

var framework = services.GetRequiredService<CommandManager>();
var parser = new StringParser();

while (true)
{
    var input = parser.Parse(Console.ReadLine());

    var context = new CommandContext();

    framework.TryExecute(context, input);
}