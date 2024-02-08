// This sample implements the same structure as https://github.com/csmir/Commands.NET/wiki/Quick-Guide.
// It also implements examples for Precondition and TypeConverter documentation.

using Commands.Core;
using Commands.Helpers;
using Commands.Parsing;
using Commands.Samples;

using Microsoft.Extensions.DependencyInjection;

var collection = new ServiceCollection()
    .ConfigureCommands(configure => configure
        .AddResultResolver((c, r, p) => Console.WriteLine(r))
        .AddTypeConverter<ReflectionTypeConverter>());

var services = collection.BuildServiceProvider();

var framework = services.GetRequiredService<CommandManager>();

while (true)
{
    var input = StringParser.Parse(Console.ReadLine()!);

    var context = new ConsumerBase();

    framework.TryExecute(context, input);
}