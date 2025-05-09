﻿using Commands;
using Commands.Tests;

var manager = new ExecutableComponentSet(new DelegateResultHandler<ConsoleCallerContext>((c, e, s) => c.Respond(e)))
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

while (true)
    await manager.Execute(new ConsoleCallerContext(Console.ReadLine()));