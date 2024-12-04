using Commands;

await CLIManager.CreateDefaultBuilder()
    .AddCommand(() => "Provide CLI arguments to execute other commands!")
    .AddCommand("hello-world", () => "Hello world!")
    .AddModule("subcommands", m => m.AddCommand("bye-world", () => "Bye world!"))
    .Run(args);