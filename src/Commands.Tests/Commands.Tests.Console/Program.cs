using Commands;
using Commands.Testing;

var components = new ComponentSetBuilder()
    .AddComponentTypes(typeof(Program).Assembly.GetExportedTypes())
    .AddResultHandler(ResultHandler.From<ICallerContext>((c, e, s) => c.Respond(e)))
    .AddComponent(
        Command.From((CommandContext<ConsoleCallerContext> c) =>
        {
            foreach (var command in c.Manager!.GetCommands())
                c.Respond(command);

        }, "help"))
    .Build();

var tests = components.GetCommands().Select(x => TestGroup.From(x).Build());

foreach (var test in tests)
{
    var result = await test.Run(x => new ConsoleCallerContext(x));

    if (result.Any(x => !x.Success))
        throw new InvalidOperationException($"A command test failed to evaluate to success. Command: {test.Command}. Test: {result.FirstOrDefault(x => !x.Success).Test}");
}

while (true)
    await components.Execute(new ConsoleCallerContext(Console.ReadLine()));