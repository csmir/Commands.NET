namespace Commands
{
    /// <summary>
    ///     Represents the tree for executing CLI commands.
    /// </summary>
    public static class CLITree
    {
        /// <summary>
        ///     Runs the provided <see cref="CLIOptions{T}"/> as a command.
        /// </summary>
        /// <param name="tree">The <see cref="ComponentTree"/> instance that should be used to run the CLI command.</param>
        /// <param name="options">The options that set up a single command execution.</param>
        /// <returns>An asynchronous <see cref="Task"/> containing the state of the command execution.</returns>
        public static Task Run<T>(this ComponentTree tree, CLIOptions<T> options)
            where T : ConsoleCallerContext, new()
        {
            var args = CommandParser.ParseKeyValueCollection(options.Arguments);

            options.Caller ??= new T();

            return tree.Execute(options.Caller, args, options.Options);
        }

        /// <summary>
        ///     Runs the current builder with the provided caller.
        /// </summary>
        /// <param name="tree">The <see cref="ComponentTree"/> instance that should be used to run the CLI command.</param>
        /// <param name="caller">The caller that represents the source of this execution.</param>
        /// <param name="args">The CLI arguments that should be used to execute a command.</param>
        /// <returns>An asynchronous <see cref="Task"/> containing the state of the command execution.</returns>
        public static Task Run<T>(this ComponentTree tree, T caller, string[] args)
            where T : ConsoleCallerContext, new()
        {
            var options = new CLIOptions<T>(caller)
            {
                Arguments = args
            };

            return tree.Run(options);
        }

        /// <summary>
        ///     Creates a builder that is responsible for setting up all required arguments to discover and populate the <see cref="ComponentTree"/>.
        /// </summary>
        /// <returns>A new <see cref="ComponentTreeBuilder"/> that builds into a new instance of <see cref="ComponentTree"/> based on the provided arguments.</returns>
        public static ComponentTreeBuilder CreateBuilder()
            => new()
            {
                Properties = new()
                {
                    ["CoreCommandName"] = "env-core"
                }
            };
    }
}
