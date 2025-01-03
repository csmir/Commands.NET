using Commands;
using Commands.Builders;

CLITree.CreateBuilder()
    .AddCommand(() => "Provide CLI arguments to execute other commands!")
    .AddCommand(c => c
        .WithHandler(() => "Hello world!")
        .WithAliases("hello-world"))
    .AddCommandGroup(m => m
        .AddCommand("bye-world", () => "Bye world!")
        .WithAliases("subcommands"))
    .Run(new ConsoleCallerContext(), args);