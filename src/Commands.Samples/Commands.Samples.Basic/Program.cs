using Commands;

await CLIManager.CreateDefaultBuilder()
    .AddCommand(() => "Provide CLI arguments to execute other commands!")
    .AddCommand("hello-world", () => "Hello world!")
    .Run(args);