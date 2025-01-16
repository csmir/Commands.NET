using Commands;
using Commands.Parsing;
using Commands.Samples;

var properties = ComponentManager.From();

properties.Configuration(ComponentConfiguration.From()
    .Parser(new SystemTypeParser(true))
    .Parser(TypeParser.From<Version>(Version.TryParse)));

properties.Handler(ResultHandler.From<ConsoleCallerContext>((caller, result, services) => caller.Respond(result.GetMessage())));

properties.Types(typeof(Program).Assembly.GetExportedTypes());

properties.Component(CommandGroup.From("greet")
    .Components(
        Command.From((CommandContext<ConsoleCallerContext> ctx) => $"Hello, {ctx.Caller.Name}"),
        Command.From((string name) => $"Hello, {name}", "another")));

var manager = properties.ToManager();

while (true)
{
    var input = Console.ReadLine()!;

    var caller = new ConsoleCallerContext(name: "Pete");

    manager.TryExecute(caller, input);
}