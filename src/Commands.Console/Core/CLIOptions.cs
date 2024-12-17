namespace Commands
{
    /// <summary>
    ///     Represents the options for executing a CLI command. This class cannot be inherited.
    /// </summary>
    public sealed class CLIOptions<T>
        where T : ConsoleCallerContext
    {
        /// <summary>
        ///     Gets or sets the consumer used in this operation.
        /// </summary>
        public T Caller { get; set; }

        /// <summary>
        ///     Gets or sets the options for the command execution. If not set, a new <see cref="Options"/> will be created.
        /// </summary>
        public CommandOptions? Options { get; set; }

        /// <summary>
        ///     Gets or sets all the arguments passed to the command. If not set, the default command will be ran.
        /// </summary>
        public string[] Arguments { get; set; } = [];

        /// <summary>
        ///     Creates a new <see cref="CLIOptions{T}"/> with the specified <see cref="ConsoleCallerContext"/>.
        /// </summary>
        /// <param name="caller">The caller that represents the source of this execution.</param>
        public CLIOptions(T caller)
        {
            Caller = caller;
        }
    }
}
