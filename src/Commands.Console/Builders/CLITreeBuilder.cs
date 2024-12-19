namespace Commands.Builders
{
    /// <summary>
    ///     Represents a set of extensions for the <see cref="ComponentTreeBuilder"/> class.
    /// </summary>
    public static class CLITreeBuilder
    {
        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the list of <see cref="ITreeBuilder.Components"/>. 
        /// </summary>
        /// <remarks>
        ///     This overload sets a default command name of <c>env_core</c>. This command is meant to be used as a default command for the environment, and will be triggered if no CLI arguments are provided.
        /// </remarks>
        /// <param name="builder">The command builder to add the command to.</param>
        /// <param name="commandAction">The action of the command. Delegate commands are adviced to set the first parameter to be <see cref="CommandContext{T}"/>, which holds scope and execution information of the created command during its execution. </param>
        /// <returns>The same <see cref="ComponentTreeBuilder"/> for call-chaining.</returns>
        public static ITreeBuilder AddCommand(this ITreeBuilder builder, Delegate commandAction)
        {
            var coreCommandName = builder.Configuration.Properties["CoreCommandName"] as string ?? "env_core";

            builder.AddCommand(coreCommandName, commandAction, []);

            return builder;
        }

        /// <summary>
        ///     Builds the underlying <see cref="ComponentTree"/> and runs it with the provided <see cref="CLIOptions{T}"/>.
        /// </summary>
        /// <param name="builder">The command builder to build into a manager.</param>
        /// <param name="options">The options that set up a single command execution.</param>
        /// <returns>An asynchronous <see cref="Task"/> containing the state of the command execution.</returns>
        public static Task Run<T>(this ITreeBuilder builder, CLIOptions<T> options)
            where T : ConsoleCallerContext
        {
            var coreCommandName = builder.Configuration.Properties["CoreCommandName"] as string ?? "env_core";

            var manager = builder.Build();

            if (options.Arguments == null || options.Arguments.Length == 0)
                options.Arguments = [coreCommandName];

            var args = ArgumentParser.ParseKeyValueCollection(options.Arguments);

            return manager.Execute(options.Caller, args, options.Options);
        }

        /// <summary>
        ///     Builds the underlying <see cref="ComponentTree"/> and runs it with the provided <see cref="CLIOptions{T}"/>.
        /// </summary>
        /// <param name="builder">The command builder to build into a manager.</param>
        /// <param name="caller">The caller that represents the source of this execution.</param>
        /// <param name="args">The CLI arguments that should be used to execute a command.</param>
        /// <returns>An asynchronous <see cref="Task"/> containing the state of the command execution.</returns>
        public static Task Run<T>(this ITreeBuilder builder, T caller, string[] args)
            where T : ConsoleCallerContext
        {
            var options = new CLIOptions<T>(caller)
            {
                Arguments = args
            };

            return builder.Run(options);
        }
    }
}
