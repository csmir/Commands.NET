// This sample implements the same structure as https://github.com/csmir/Commands.NET/wiki/Quick-Guide.
// It also implements examples for Precondition and TypeConverter documentation.

using Commands;
using Commands.Parsing;
using Commands.Samples;

var builder = CommandManager.CreateBuilder();

// Adds a resolver to send errors into the console or do something else with them.
builder.AddResultResolver((consumer, result, services) =>
{
    if (!result.Success)
    {
        Console.WriteLine(result);
    }
});

// Adds a converter to convert an input, in this case, a string, into a custom type.`
builder.AddTypeConverter(new ReflectionTypeConverter(caseIgnore: true));

// Add commands to the builder directly:
builder.AddCommand("delegate", () => "Hello World!");
builder.AddCommand("delegate-context", (CommandContext<CustomConsumer> ctx) => $"Hello, {ctx.Consumer.Name}!");
builder.AddCommand("delegate-params", (string name) => $"Hello, {name}!");

// This API is useful for lightweight applications that do not need to do any complex operations, but still wanting to keep command execution simple and without boilerplate.

// Builds the preparation models into a framework to operate over.
var framework = builder.Build();

while (true)
{
    // Creates a consumer to execute the input. This consumer is the same as a Context in other frameworks.
    var consumer = new CustomConsumer(name: "Harold");

    // Executes the input.
    await framework.Execute(consumer, Console.ReadLine()!);
}