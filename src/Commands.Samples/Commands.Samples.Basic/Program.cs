using Commands;
using Commands.Console;

new CLIBuilder<CommandManager, ConsoleConsumerBase>(args)
    .AddCommand(() =>
    {
        return "Provide CLI arguments to execute other commands!";
    })
    .AddCommand("hello-world", () =>
    {
        return "Hello, world!";
    })
    .BuildAndRun();