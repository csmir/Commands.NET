using Commands.Converters;
using Commands.Parsing;
using Commands.Resolvers;

namespace Commands
{
    /// <summary>
    ///     Represents the manager for executing CLI commands.
    /// </summary>
    public static class CLIManager
    {
        /// <summary>
        ///     Runs the provided <see cref="CLIOptions"/> as a command.
        /// </summary>
        /// <param name="manager">The <see cref="CommandTree"/> instance that should be used to run the CLI command.</param>
        /// <param name="options">The options that set up a single command execution.</param>
        /// <returns>An asynchronous <see cref="Task"/> containing the state of the command execution.</returns>
        public static Task Run(this CommandTree manager, CLIOptions options)
        {
            var args = StringParser.ParseKeyValueCollection(options.CommandArguments);

            options.Consumer ??= new ConsoleConsumerBase();

            return manager.Execute(options.Consumer, args, options.CommandOptions);
        }

        /// <summary>
        ///     Sets the provided <paramref name="args"/> in a <see cref="CLIOptions"/> with default settings, and runs them as a command.
        /// </summary>
        /// <param name="manager">The <see cref="CommandTree"/> instance that should be used to run the CLI command.</param>
        /// <param name="args">The CLI arguments that should be used to execute a command.</param>
        /// <returns>An asynchronous <see cref="Task"/> containing the state of the command execution.</returns>
        public static Task Run(this CommandTree manager, string[] args)
        {
            var options = new CLIOptions
            {
                CommandArguments = args
            };

            return manager.Run(options);
        }

        /// <summary>
        ///     Creates a builder that is responsible for setting up all required arguments to discover and populate the <see cref="CommandTree"/>.
        /// </summary>
        /// <remarks>
        ///     This builder is able to configure the following:
        ///     <list type="number">
        ///         <item>Defining assemblies through which will be searched to discover modules and commands.</item>
        ///         <item>Defining custom commands that do not appear in the assemblies.</item>
        ///         <item>Registering implementations of <see cref="TypeConverterBase"/> which define custom argument conversion.</item>
        ///         <item>Registering implementations of <see cref="ResultResolverBase"/> which define custom result handling.</item>
        ///         <item>Custom naming patterns that validate naming across the whole process.</item>
        ///     </list>
        /// </remarks>
        /// <returns>A new <see cref="ConfigurationBuilder"/> that implements <see cref="CommandTree"/></returns>
        public static ConfigurationBuilder CreateDefaultBuilder()
        {
            return new ConfigurationBuilder();
        }
    }
}
