using Commands;

await CLITree.CreateBuilder()
    .AddCommand(() => "Provide CLI arguments to execute other commands!")
    .AddCommand(c => c
        .WithDelegate(() => "Hello world!")
        .WithAliases("hello-world"))
    .AddModule(m => m
        .AddCommand("bye-world", () => "Bye world!")
        .WithAliases("subcommands"))
    .Run(new ConsoleCallerContext(), args);