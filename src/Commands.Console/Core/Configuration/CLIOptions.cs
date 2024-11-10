namespace Commands
{
    /// <summary>
    ///     Represents the options for executing a CLI command.
    /// </summary>
    public class CLIOptions
    {
        /// <summary>
        ///     Gets or sets the options for the command execution. If not set, a new <see cref="CommandOptions"/> will be created.
        /// </summary>
        public CommandOptions? CommandOptions { get; set; }

        /// <summary>
        ///     Gets or sets the consumer used in this operation. If not set, a new <see cref="ConsoleConsumerBase"/> will be created.
        /// </summary>
        public ConsoleConsumerBase? Consumer { get; set; }

        /// <summary>
        ///     Gets or sets all the arguments passed to the command. If not set, the default command will be ran.
        /// </summary>
        public string[] CommandArguments { get; set; } = [];
    }
}
