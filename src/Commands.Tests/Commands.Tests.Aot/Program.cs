using Commands;
using Commands.Tests;

var manager = new ComponentManager(new DelegateResultHandler<ConsoleContext>((c, r, s) => c.Respond(r.GetMessage())))
{
    new CommandGroup(typeof(Module), configuration: new ComponentConfiguration()),
    new CommandGroup("commandgroup")
    {
        new Command(() => "Hello, user!", "subcommand"),
    },
    new Command(() => Environment.Exit(0), "exit"),
    new Command(async (CommandContext<ConsoleContext> context, int timeout) => 
    { 
        await context.Respond($"Waiting for {timeout} ms..."); 
        await Task.Delay(timeout); 

        return "Async timeout completed."; 

    }, "asyncwork"),
};

while (true)
    manager.TryExecute(new ConsoleContext(), Console.ReadLine()!, new CommandOptions()
    {
        AsynchronousExecution = true,
    });