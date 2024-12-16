namespace Commands
{
    /// <summary>
    ///     Represents a set of extensions for the <see cref="CommandTreeBuilder"/> class.
    /// </summary>
    public static class CLITreeBuilder
    {
        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the list of <see cref="CommandTreeBuilder.Components"/>. 
        /// </summary>
        /// <remarks>
        ///     This overload sets a default command name of <c>env_core</c>. This command is meant to be used as a default command for the environment.
        /// </remarks>
        /// <param name="builder">The command builder to add the command to.</param>
        /// <param name="commandAction">The action of the command. Delegate commands are adviced to set the first parameter to be <see cref="CommandContext{T}"/>, which holds scope and execution information of the created command during its execution. </param>
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        public static CommandTreeBuilder AddCommand(this CommandTreeBuilder builder, Delegate commandAction)
        {
            builder.AddCommand("env_core", commandAction, []);

            return builder;
        }

        /// <summary>
        ///     Builds the underlying <see cref="CommandTree"/> and runs it with the provided <see cref="CLIOptions"/>.
        /// </summary>
        /// <param name="builder">The command builder to build into a manager.</param>
        /// <param name="options">The options that set up a single command execution.</param>
        /// <returns>An asynchronous <see cref="Task"/> containing the state of the command execution.</returns>
        public static Task Run(this CommandTreeBuilder builder, CLIOptions options)
        {
            var manager = builder.Build();

            if (options.CommandArguments == null || options.CommandArguments.Length == 0)
            {
                options.CommandArguments = ["env_core"];
            }

            var args = CommandParser.ParseKeyValueCollection(options.CommandArguments);

            options.Consumer ??= new ConsoleCallerContext();

            return manager.Execute(options.Consumer, args, options.CommandOptions);
        }

        /// <summary>
        ///     Builds the underlying <see cref="CommandTree"/> and runs it with the provided <see cref="CLIOptions"/>.
        /// </summary>
        /// <param name="builder">The command builder to build into a manager.</param>
        /// <param name="args">The CLI arguments that should be used to execute a command.</param>
        /// <returns>An asynchronous <see cref="Task"/> containing the state of the command execution.</returns>
        public static Task Run(this CommandTreeBuilder builder, string[] args)
        {
            var options = new CLIOptions
            {
                CommandArguments = args
            };

            return builder.Run(options);
        }
    }
}
