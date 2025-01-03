// This sample implements the same structure as https://github.com/csmir/Commands.NET/wiki/Quick-Guide.

using Commands;
using Commands.Conversion;
using Commands.Samples;

var tree = ComponentTree.CreateBuilder()
    .ConfigureComponents(configure =>
    {
        configure.AddParser(new LiteralTypeParser(caseIgnore: true));
        configure.AddParser(new TryParseParser<Version>(Version.TryParse));
    })
    .AddResultHandler((caller, result, services) => caller.Respond(result))
    .WithTypes(typeof(Program).Assembly.GetExportedTypes())
    .AddCommandGroup(x => x
        .WithNames("greet")
        .AddCommand(x => x
            .WithNames("self")
            .WithHandler((CommandContext<ConsoleCaller> ctx) => $"Hello, {ctx.Caller.Name}"))
        .AddCommand(x => x
            .WithNames("another")
            .WithHandler((string name) => $"Hello, {name}")))
    .Build();

while (true)
{
    var input = Console.ReadLine()!;

    var consumer = new ConsoleCaller(name: "Harold");

    tree.Execute(consumer, input);
}