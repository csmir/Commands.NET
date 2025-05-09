using Commands;

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

while (true)
    await components.Execute(new ConsoleCallerContext(Console.ReadLine()));