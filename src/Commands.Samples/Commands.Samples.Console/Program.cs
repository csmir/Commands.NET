using Commands;
using Commands.Parsing;
using Commands.Samples;

var properties = ComponentManager.From();

properties.Configuration(ComponentConfiguration.From()
    .Parser(new SystemTypeParser(true))
    .Parser(TypeParser.From<Version>(Version.TryParse)));

properties.Handler(ResultHandler.From<ConsoleCallerContext>((caller, result, services) => caller.Respond(result.Unfold())));

properties.Types(typeof(Program).Assembly.GetExportedTypes());

properties.Component(CommandGroup.From("greet")
    .Components(
        Command.From((CommandContext<ConsoleCallerContext> ctx) => $"Hello, {ctx.Caller.Name}"),
        Command.From((string name) => $"Hello, {name}", "another")));

var manager = properties.Create();

while (true)
    await manager.ExecuteBlocking(new ConsoleCallerContext("Pete", Console.ReadLine()));