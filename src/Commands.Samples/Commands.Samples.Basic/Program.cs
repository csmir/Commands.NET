using Commands;
using Commands.Builders;

CLITree.CreateBuilder()
    .AddCommand(() => "Provide CLI arguments to execute other commands!")
    .AddCommand(c => c
        .WithHandler(() => "Hello world!")
        .WithNames("hello-world"))
    .AddCommandGroup(m => m
        .AddCommand("bye-world", () => "Bye world!")
        .WithNames("subcommands"))
    .Run(new ConsoleCallerContext(), args);