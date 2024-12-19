using Commands.Conversion;

namespace Commands.Builders
{
    /// <summary>
    ///     A base class that represents a delegate based command, before it is built into a reflection-based executable object. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     This class is used to configure a command before it is built into a <see cref="CommandInfo"/> object. By calling the <see cref="Build(ComponentConfiguration)"/> or <see cref="Build(IEnumerable{TypeParser}, string)"/> method, the command is built into an object that can be executed by the <see cref="ComponentTree"/>>.
    /// </remarks>
    public sealed class CommandBuilder : IComponentBuilder
    {
        private readonly bool _isNested;

        private static readonly Type c_type = typeof(CommandContext<>);

        /// <inheritdoc />
        public string[] Aliases { get; set; } = [];

        /// <summary>
        ///     Gets the conditions necessary for the command to execute.
        /// </summary>
        public List<IExecuteCondition> Conditions { get; set; } = [];

        /// <summary>
        ///     Gets the delegate that is executed when the command is invoked.
        /// </summary>
        public Delegate ExecuteDelegate { get; set; } = default!;

        /// <summary>
        ///     Creates a new instance of <see cref="CommandBuilder"/>.
        /// </summary>
        public CommandBuilder() { }

        /// <summary>
        ///     Creates a new instance of <see cref="CommandBuilder"/> with the specified name, aliases, and delegate.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="aliases">The aliases of the command, excluding the name.</param>
        /// <param name="executeDelegate">The delegate used to execute the command.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="executeDelegate"/> is <see langword="null"/>, or when the aliases are <see langword="null"/>.</exception>
        public CommandBuilder(string name, string[] aliases, Delegate executeDelegate)
        {
            aliases = new string[] { name }
                .Concat(aliases)
                .Distinct()
                .ToArray();

            Aliases = aliases;
            ExecuteDelegate = executeDelegate;

            Conditions = [];
        }

        internal CommandBuilder(Delegate executeDelegate)
            : this(true)
        {
            ExecuteDelegate = executeDelegate;
        }

        internal CommandBuilder(bool isNested)
        {
            _isNested = isNested;
        }

        /// <summary>
        ///     Replaces the current collection of aliases with the specified aliases. Aliases are used to identify the command in the command execution pipeline.
        /// </summary>
        /// <param name="aliases">The aliases of the command.</param>
        /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
        public CommandBuilder WithAliases(params string[] aliases)
        {
            Aliases = aliases;

            return this;
        }

        /// <summary>
        ///     Replaces the current delegate with the specified delegate. The delegate is executed when the command is invoked.
        /// </summary>
        /// <param name="executionDelegate">The delegate to be executed when the command is invoked.</param>
        /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
        public CommandBuilder WithDelegate(Delegate executionDelegate)
        {
            ExecuteDelegate = executionDelegate;

            return this;
        }

        /// <summary>
        ///     Replaces the current collection of conditions with the specified conditions. Conditions are used to determine if the command can be executed.
        /// </summary>
        /// <param name="conditions">The conditions to add to the command execution flow.</param>
        /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
        public CommandBuilder WithConditions(params IExecuteCondition[] conditions)
        {
            Conditions = [.. conditions];
            return this;
        }

        /// <summary>
        ///     Adds a condition to the command. Conditions are used to determine if the command can be executed.
        /// </summary>
        /// <param name="condition">The condition to add to the command execution flow.</param>
        /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
        public CommandBuilder AddCondition(IExecuteCondition condition)
        {
            Conditions.Add(condition);
            return this;
        }

        /// <inheritdoc />
        public IComponent Build(ComponentConfiguration configuration)
        {
            if (ExecuteDelegate is null)
                throw new ArgumentNullException(nameof(ExecuteDelegate));

            if (!_isNested && Aliases.Length == 0)
                throw BuildException.AliasAtLeastOne();

            if (configuration.NamingPattern is not null)
            {
                foreach (var alias in Aliases)
                {
                    if (!configuration.NamingPattern.IsMatch(alias))
                        throw BuildException.AliasConvention(alias);
                }
            }

            var param = ExecuteDelegate.Method.GetParameters();

            var hasContext = false;

            if (param.Length > 0 && param[0].ParameterType.IsGenericType && param[0].ParameterType.GetGenericTypeDefinition() == c_type)
                hasContext = true;

            return new CommandInfo(new DelegateActivator(ExecuteDelegate.Method, ExecuteDelegate.Target, hasContext), [.. Conditions], Aliases, hasContext, configuration);
        }

        /// <summary>
        ///     Builds a searchable component from the provided configuration.
        /// </summary>
        /// <param name="converters">The typeconverters from which the current command constructs its argument converters.</param>
        /// <param name="nameFilter">A filter which is used to determine how the command aliases are validated.</param>
        /// <returns>A reflection-based container that holds information for a component ready to be executed or serves as a container for executable components.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the component aliases do not match <see cref="ComponentConfiguration.NamingPattern"/>.</exception>
        public IComponent Build(IEnumerable<TypeParser> converters, string? nameFilter = @"^[a-z0-9_-]*$")
            => Build(new ComponentConfiguration(converters, nameFilter));
    }
}
