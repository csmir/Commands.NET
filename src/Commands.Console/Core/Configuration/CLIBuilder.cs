using Commands.Parsing;

namespace Commands
{
    /// <summary>
    ///     Represents a set of extensions for the <see cref="CommandBuilder{T}"/> class.
    /// </summary>
    public static class CLIBuilder
    {
        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the list of <see cref="CommandBuilder.Commands"/>. 
        /// </summary>
        /// <remarks>
        ///     This overload sets a default command name of <c>env_core</c>. This command is meant to be used as a default command for the environment.
        /// </remarks>
        /// <param name="builder">The command builder to add the command to.</param>
        /// <param name="commandAction">The action of the command. Delegate commands are adviced to set the first parameter to be <see cref="CommandContext{T}"/>, which holds scope and execution information of the created command during its execution. </param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public static CommandBuilder<T> AddCommand<T>(this CommandBuilder<T> builder, Delegate commandAction)
            where T : CommandManager
        {
            builder.AddCommand("env_core", commandAction, []);

            return builder;
        }

        /// <summary>
        ///     Builds the underlying <see cref="CommandManager"/> and runs it with the provided <see cref="CLIOptions"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="CommandManager"/> that should be built, and a single command executed within it.</typeparam>
        /// <param name="builder">The command builder to build into a manager.</param>
        /// <param name="options">The options that set up a single command execution.</param>
        /// <param name="ctorArgs">The constructor arguments that are required to construct a new instance of the command manager, if any.</param>
        /// <returns>An asynchronous <see cref="Task"/> containing the state of the command execution.</returns>
        public static Task Run<T>(this CommandBuilder<T> builder, CLIOptions options, params object[] ctorArgs)
            where T : CommandManager
        {
            var manager = builder.Build(ctorArgs);

            if (options.CommandArguments == null || options.CommandArguments.Length == 0)
            {
                options.CommandArguments = ["env_core"];
            }

            var args = StringParser.ParseKeyValueCollection(options.CommandArguments);

            options.Consumer ??= new ConsoleConsumerBase();

            return manager.Execute(options.Consumer, args, options.CommandOptions);
        }

        /// <summary>
        ///     Builds the underlying <see cref="CommandManager"/> and runs it with the provided <see cref="CLIOptions"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="CommandManager"/> that should be built, and a single command executed within it.</typeparam>
        /// <param name="builder">The command builder to build into a manager.</param>
        /// <param name="options">The options that set up a single command execution.</param>
        /// <returns>An asynchronous <see cref="Task"/> containing the state of the command execution.</returns>
        public static Task Run<T>(this CommandBuilder<T> builder, CLIOptions options)
            where T : CommandManager
        {
            return builder.Run(options, []);
        }

        /// <summary>
        ///     Builds the underlying <see cref="CommandManager"/> and runs it with the provided <see cref="CLIOptions"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="CommandManager"/> that should be built, and a single command executed within it.</typeparam>
        /// <param name="builder">The command builder to build into a manager.</param>
        /// <param name="args">The CLI arguments that should be used to execute a command.</param>
        /// <returns>An asynchronous <see cref="Task"/> containing the state of the command execution.</returns>
        public static Task Run<T>(this CommandBuilder<T> builder, string[] args)
            where T : CommandManager
        {
            var options = new CLIOptions
            {
                CommandArguments = args
            };

            return builder.Run(options);
        }
    }
}
