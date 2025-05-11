using Commands;

var components = new ComponentTree()
{
    new Command((CommandContext<ConsoleCallerContext> c) =>
    {
        foreach (var command in c.Provider!.Components.GetCommands())
            c.Respond(command);

    }, "help")
};

components.AddRange(typeof(Program).Assembly.GetExportedTypes());

var provider = new ComponentProvider(components);

while (true)
    await provider.Execute(new ConsoleCallerContext(Console.ReadLine()));