using Commands;
using Commands.Parsing;
using Commands.Samples;

var properties = ComponentManager.Define();

properties.Configuration(ComponentConfiguration.Define()
    .Parser(TypeParser.Define(new SystemTypeParser(true)))
    .Parser(TypeParser.Define<Version>().Delegate(Version.TryParse)));

properties.Handler(ResultHandler.Define<ConsoleCallerContext>()
    .Delegate((caller, result, services) => caller.Respond(result)));

properties.Types(typeof(Program).Assembly.GetExportedTypes());

properties.Component(CommandGroup.Define()
    .Name("greet")
    .Component(Command.Define()
        .Name("self")
        .Handler((CommandContext<ConsoleCallerContext> ctx) => $"Hello, {ctx.Caller.Name}"))
    .Component(Command.Define()
        .Name("another")
        .Handler((string name) => $"Hello, {name}")));

var manager = properties.ToManager();

while (true)
{
    var input = Console.ReadLine()!;

    var caller = new ConsoleCallerContext(name: "Pete");

    manager.TryExecute(caller, input);
}