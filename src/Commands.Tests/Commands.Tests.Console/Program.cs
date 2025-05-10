using Commands;

var provider = new ComponentProviderBuilder()
    .AddResultHandler<ICallerContext>((c, e, s) => c.Respond(e))
    .AddComponentTypes(typeof(Program).Assembly.GetExportedTypes())
    .AddComponent(new Command((CommandContext<ConsoleCallerContext> c) =>
    {
        foreach (var command in c.Provider!.Components.GetCommands())
            c.Respond(command);

    }, "help"))
    .Build();

while (true)
    await provider.Execute(new ConsoleCallerContext(Console.ReadLine()));