namespace Commands
{
    /// <summary>
    ///     Represents the tree for executing CLI commands.
    /// </summary>
    public static class CLITree
    {
        /// <summary>
        ///     Runs the provided <see cref="CLIOptions"/> as a command.
        /// </summary>
        /// <param name="tree">The <see cref="CommandTree"/> instance that should be used to run the CLI command.</param>
        /// <param name="options">The options that set up a single command execution.</param>
        /// <returns>An asynchronous <see cref="Task"/> containing the state of the command execution.</returns>
        public static Task Run(this CommandTree tree, CLIOptions options)
        {
            var args = CommandParser.ParseKeyValueCollection(options.CommandArguments);

            options.Consumer ??= new ConsoleCallerContext();

            return tree.Execute(options.Consumer, args, options.CommandOptions);
        }

        /// <summary>
        ///     Sets the provided <paramref name="args"/> in a <see cref="CLIOptions"/> with default settings, and runs them as a command.
        /// </summary>
        /// <param name="tree">The <see cref="CommandTree"/> instance that should be used to run the CLI command.</param>
        /// <param name="args">The CLI arguments that should be used to execute a command.</param>
        /// <returns>An asynchronous <see cref="Task"/> containing the state of the command execution.</returns>
        public static Task Run(this CommandTree tree, string[] args)
        {
            var options = new CLIOptions
            {
                CommandArguments = args
            };

            return tree.Run(options);
        }

        /// <summary>
        ///     Creates a builder that is responsible for setting up all required arguments to discover and populate the <see cref="CommandTree"/>.
        /// </summary>
        /// <returns>A new <see cref="CommandTreeBuilder"/> that builds into a new instance of <see cref="CommandTree"/> based on the provided arguments.</returns>
        public static CommandTreeBuilder CreateDefaultBuilder()
            => new();
    }
}
