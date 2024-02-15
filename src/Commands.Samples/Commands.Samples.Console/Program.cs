// This sample implements the same structure as https://github.com/csmir/Commands.NET/wiki/Quick-Guide.
// It also implements examples for Precondition and TypeConverter documentation.

using Commands;
using Commands.Parsing;
using Commands.Samples;

var builder = CommandManager.CreateBuilder();

builder.AddResultResolver((consumer, result, services) =>
{
    if (!result.Success)
    {
        Console.WriteLine(result);
    }
});

builder.AddTypeConverter(new ReflectionTypeConverter(caseIgnore: true));

var framework = builder.Build();

while (true)
{
    var input = StringParser.Parse(Console.ReadLine()!);

    var consumer = new ConsumerBase();

    await framework.TryExecuteAsync(consumer, input);
}