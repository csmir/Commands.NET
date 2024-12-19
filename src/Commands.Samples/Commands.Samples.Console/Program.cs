// This sample implements the same structure as https://github.com/csmir/Commands.NET/wiki/Quick-Guide.

using Commands;
using Commands.Samples;

var builder = ComponentTree.CreateBuilder();

builder.AddResultResolver((consumer, result, services) =>
{
    if (!result.Success)
    {
        consumer.Respond(result);
    }
});

builder.Configuration.AddParser<Version>((consumer, argument, value, services) =>
{
    if (Version.TryParse(value?.ToString(), out var version))
    {
        return ConvertResult.FromSuccess(version);
    }

    return ConvertResult.FromError(new FormatException("Invalid version format."));
});

builder.Configuration.AddParser(new LiteralTypeParser(caseIgnore: true));

builder.AddCommand("delegate", () => "Hello World!");
builder.AddCommand("delegate-context", (CommandContext<CustomCaller> ctx) => $"Hello, {ctx.Caller.Name}!");
builder.AddCommand("delegate-params", (string name) => $"Hello, {name}!");

var manager = builder.Build();

while (true)
{
    var input = Console.ReadLine()!;

    var consumer = new CustomCaller(name: "Harold");

    await manager.Execute(consumer, input);
}