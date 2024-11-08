using Commands.Helpers;

namespace Commands.Console
{
    /// <summary>
    ///     Represents a command builder meant for a single command execution through the command line interface.
    /// </summary>
    /// <param name="args">The set of arguments provided to the program.</param>
    public class CLIBuilder<T, TConsumer>(params string[] args) : CommandBuilder<T>()
        where TConsumer : ConsoleConsumerBase, new()
        where T : CommandManager
    {
        /// <summary>
        ///     Gets or sets the arguments that should be passed to the initiated command.
        /// </summary>
        public string[] CommandArguments { get; set; } = args;

        /// <summary>
        ///     Gets or sets the consumer that should be used to interact with the console.
        /// </summary>
        public TConsumer Consumer { get; set; } = new TConsumer();

        /// <summary>
        ///     Gets or sets the options that should be used to configure the command execution.
        /// </summary>
        public CommandOptions Options { get; set; } = new CommandOptions();

        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the list of <see cref="CommandBuilder{T}.Commands"/>.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="commandAction">The action of the command. Delegate commands are adviced to set the first parameter to be <see cref="CommandContext{T}"/>, which holds scope and execution information of the created command during its execution.</param>
        /// <param name="aliases">The aliases of the command.</param>
        /// <returns>The same <see cref="CLIBuilder{T, TConsumer}"/> for call-chaining.</returns>
        public new CLIBuilder<T, TConsumer> AddCommand(string name, Delegate commandAction, params string[] aliases)
        {
            base.AddCommand(name, commandAction, aliases);

            return this;
        }

        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the list of <see cref="CommandBuilder{T}.Commands"/>. 
        /// </summary>
        /// <remarks>
        ///     This overload sets a default command name of <c>env_core</c>. This command is meant to be used as a default command for the environment.
        /// </remarks>
        /// <param name="commandAction">The action of the command. Delegate commands are adviced to set the first parameter to be <see cref="CommandContext{T}"/>, which holds scope and execution information of the created command during its execution. </param>
        /// <returns>The same <see cref="CLIBuilder{T, TConsumer}"/> for call-chaining.</returns>
        public CLIBuilder<T, TConsumer> AddCommand(Delegate commandAction)
        {
            base.AddCommand("env_core", commandAction, []);

            return this;
        }

        /// <summary>
        ///     Configures the <see cref="CommandOptions"/> that should be used to configure the command execution.
        /// </summary>
        /// <param name="options">The options that should be used when executing a CLI command.</param>
        /// <returns>The same <see cref="CLIBuilder{T, TConsumer}"/> for call-chaining.</returns>
        public CLIBuilder<T, TConsumer> WithOptions(CommandOptions options)
        {
            if (options is null)
            {
                ThrowHelpers.ThrowInvalidArgument(options);
            }

            Options = options;

            return this;
        }

        /// <summary>
        ///     Builds the underlying <see cref="CommandManager"/>, and runs a command from the args provided by <see cref="CommandArguments"/> and <see cref="Options"/>.
        /// </summary>
        public void BuildAndRun()
        {
            BuildAndRunAsync([]).Wait();
        }

        /// <summary>
        ///     Builds the underlying <see cref="CommandManager"/>, and runs a command from the args provided by <see cref="CommandArguments"/> and <see cref="Options"/>.
        /// </summary>
        /// <param name="ctorArgs">The arguments that should be passed to the constructor of the <typeparamref name="T"/> command manager.</param>
        public void BuildAndRun(params string[] ctorArgs)
        {
            BuildAndRunAsync(ctorArgs).Wait();
        }

        /// <summary>
        ///     Builds the underlying <see cref="CommandManager"/>, and runs a command from the args provided by <see cref="CommandArguments"/> and <see cref="Options"/>.
        /// </summary>
        /// <returns>An asynchronous <see cref="Task"/> that represents the state of the executing CLI command.</returns>
        public Task BuildAndRunAsync()
        {
            return BuildAndRunAsync([]);
        }

        /// <summary>
        ///     Builds the underlying <see cref="CommandManager"/>, and runs a command from the args provided by <see cref="CommandArguments"/> and <see cref="Options"/>.
        /// </summary>
        /// <param name="ctorArgs">The arguments that should be passed to the constructor of the <typeparamref name="T"/> command manager.</param>
        /// <returns>An asynchronous <see cref="Task"/> that represents the state of the executing CLI command.</returns>
        public Task BuildAndRunAsync(params string[] ctorArgs)
        {
            var manager = Build(ctorArgs);

            if (Consumer is null)
            {
                ThrowHelpers.ThrowInvalidArgument(Consumer);
            }

            if (CommandArguments is null || CommandArguments.Length == 0)
            {
                CommandArguments = ["env_core"];
            }

            return manager.Execute(Consumer, CommandArguments, Options);
        }
    }
}
