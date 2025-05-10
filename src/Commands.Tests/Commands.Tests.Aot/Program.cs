using Commands;
using Commands.Tests;

var components = new ComponentTree()
{
    new CommandGroup<Module>(),
    new CommandGroup("commandgroup")
    {
        new Command(() => "Hello, user!", "subcommand"),
    },
    new Command(() => Environment.Exit(0), "exit"),
    new Command(async (CommandContext<ConsoleCallerContext> context, int timeout) =>
    {
        await context.Respond($"Waiting for {timeout} ms...");
        await Task.Delay(timeout);

        return "Async timeout completed.";

    }, "asyncwork"),
};

var provider = new ComponentProvider(components, new DelegateResultHandler<ConsoleCallerContext>((c, e, s) => c.Respond(e)));

while (true)
    await provider.Execute(new ConsoleCallerContext(Console.ReadLine()));