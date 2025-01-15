using Commands;
using Commands.Parsing;
using Commands.Samples;

var properties = ComponentManager.From();

properties.Configuration(ComponentConfiguration.From()
    .Parser(TypeParser.From(new SystemTypeParser(true)))
    .Parser(TypeParser.From<Version>().Delegate(Version.TryParse)));

properties.Handler(ResultHandler.From<ConsoleCallerContext>()
    .Delegate((caller, result, services) => caller.Respond(result)));

properties.Types(typeof(Program).Assembly.GetExportedTypes());

properties.Component(CommandGroup.From()
    .Name("greet")
    .Component(Command.From()
        .Name("self")
        .Handler((CommandContext<ConsoleCallerContext> ctx) => $"Hello, {ctx.Caller.Name}"))
    .Component(Command.From()
        .Name("another")
        .Handler((string name) => $"Hello, {name}")));

var manager = properties.ToManager();

while (true)
{
    var input = Console.ReadLine()!;

    var caller = new ConsoleCallerContext(name: "Pete");

    manager.TryExecute(caller, input);
}