using Commands.Reflection;

namespace Commands
{
    /// <summary>
    ///     A base class that represents a delegate based command, before it is built into a reflection-based executable object. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     This class is used to configure a command before it is built into a <see cref="CommandInfo"/> object. By calling the <see cref="Build"/> method, the command is built into an object that can be executed by the <see cref="CommandManager"/>>.
    /// </remarks>
    public sealed class CommandBase
    {
        private static readonly Type c_type = typeof(CommandContext<>);

        /// <summary>
        ///     Gets the name of the command. This is the primary alias of the command.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets all aliases of the command, including its name. This is used to identify the command in the command execution pipeline.
        /// </summary>
        public string[] Aliases { get; }

        /// <summary>
        ///     Gets the delegate that is executed when the command is invoked.
        /// </summary>
        public Delegate ExecuteDelegate { get; }

        /// <summary>
        ///     Creates a new instance of <see cref="CommandBase"/> with the specified name, aliases, and delegate.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="aliases">The aliases of the command, excluding the name.</param>
        /// <param name="executeDelegate">The delegate used to execute the command.</param>
        public CommandBase(string name, string[] aliases, Delegate executeDelegate)
        {
            if (executeDelegate == null)
                throw new ArgumentNullException(nameof(executeDelegate));

            if (aliases == null)
                throw new ArgumentNullException(nameof(aliases));

            aliases = new string[] { name }
                .Concat(aliases)
                .Distinct()
                .ToArray();

            Name = name;
            Aliases = aliases;
            ExecuteDelegate = executeDelegate;
        }

        internal CommandBase(string[] aliases, Delegate executeDelegate)
        {
            Name = aliases[0];
            Aliases = aliases;
            ExecuteDelegate = executeDelegate;
        }

        /// <summary>
        ///     Builds the command into a <see cref="CommandInfo"/> object that can be executed by the <see cref="CommandManager"/>.
        /// </summary>
        /// <param name="configuration">The configuration entity that determines the creation of a command.</param>
        /// <returns></returns>
        public CommandInfo Build(CommandConfiguration configuration)
        {
            foreach (var alias in Aliases)
            {
                if (!configuration.NamingRegex.IsMatch(alias))
                    throw new InvalidOperationException($"The alias of must match the filter provided in the {nameof(CommandConfiguration.NamingRegex)} of the {nameof(CommandConfiguration)}.");
            }

            var param = ExecuteDelegate.Method.GetParameters();

            var hasContext = false;
            if (param.Length > 0 && param[0].ParameterType.IsGenericType && param[0].ParameterType.GetGenericTypeDefinition() == c_type)
            {
                hasContext = true;
            }

            return new CommandInfo(new DelegateInvoker(ExecuteDelegate.Method, ExecuteDelegate.Target, hasContext), Aliases, hasContext, configuration);
        }
    }
}
