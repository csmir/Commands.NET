using Commands.Conditions;

namespace Commands.Builders
{
    /// <summary>
    ///     A builder model that represents the construction of a delegate based command. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     This class is used to configure a command before it is built into a <see cref="CommandInfo"/> object. 
    ///     By calling <see cref="Build(ComponentConfiguration)"/>, the command is built into an object that can be executed by the <see cref="IComponentTree"/>.
    /// </remarks>
    public sealed class CommandBuilder : IComponentBuilder
    {
        private readonly bool _isNested;

        /// <inheritdoc />
        public ICollection<string> Aliases { get; set; } = [];

        /// <inheritdoc />
        public ICollection<IConditionBuilder> Conditions { get; set; } = [];

        /// <summary>
        ///     Gets or sets the delegate that is executed when the command is invoked.
        /// </summary>
        public Delegate Handler { get; set; } = default!;

        /// <summary>
        ///     Creates a new instance of <see cref="CommandBuilder"/>.
        /// </summary>
        public CommandBuilder() { }

        /// <summary>
        ///     Creates a new instance of <see cref="CommandBuilder"/> with the specified name and delegate.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="executeDelegate">The delegate used to execute the command.</param>
        public CommandBuilder(string name, Delegate executeDelegate)
            : this(name, [], executeDelegate) { }

        /// <summary>
        ///     Creates a new instance of <see cref="CommandBuilder"/> with the specified name, aliases, and delegate.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="aliases">The aliases of the command, excluding the name.</param>
        /// <param name="executeDelegate">The delegate used to execute the command.</param>
        public CommandBuilder(string name, IEnumerable<string> aliases, Delegate executeDelegate)
        {
            var joined = new string[] { name }
                .Concat(aliases)
                .Distinct()
                .ToArray();

            Aliases = joined;
            Handler = executeDelegate;

            Conditions = [];
        }

        internal CommandBuilder(Delegate executeDelegate)
            : this(true)
        {
            Handler = executeDelegate;
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
        /// <param name="executionHandler">The delegate to be executed when the command is invoked.</param>
        /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
        public CommandBuilder WithHandler(Delegate executionHandler)
        {
            Handler = executionHandler;

            return this;
        }

        /// <summary>
        ///     Replaces the current collection of conditions with the specified conditions. Conditions are used to determine if the command can be executed.
        /// </summary>
        /// <param name="conditions">The conditions to add to the command execution flow.</param>
        /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
        public CommandBuilder WithConditions(params IConditionBuilder[] conditions)
        {
            Conditions = [.. conditions];
            return this;
        }

        /// <summary>
        ///     Adds a condition to the command. Conditions are used to determine if the command can be executed.
        /// </summary>
        /// <param name="condition">The condition to add to the command execution flow.</param>
        /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
        public CommandBuilder AddCondition(IConditionBuilder condition)
        {
            Assert.NotNull(condition, nameof(condition));

            Conditions.Add(condition);
            return this;
        }

        /// <summary>
        ///     Adds a condition to the command, which must succeed alongside other conditions with the same trigger created by this overload. Conditions are used to determine if the command can be executed.
        /// </summary>
        /// <remarks>
        ///     This overload creates a new instance of the specified condition type and configures it using the provided <paramref name="configureCondition"/> delegate.
        /// </remarks>
        /// <param name="configureCondition">A configuration delegate that should configure the condition builder.</param>
        /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
        public CommandBuilder AddCondition(Action<ConditionBuilder<ANDEvaluator, ICallerContext>> configureCondition)
            => AddCondition<ANDEvaluator, ICallerContext>(configureCondition);

        /// <summary>
        ///     Adds a condition to the command. Conditions are used to determine if the command can be executed.
        /// </summary>
        /// <remarks>
        ///     This overload creates a new instance of the specified condition type and configures it using the provided <paramref name="configureCondition"/> delegate.
        /// </remarks>
        /// <typeparam name="TEval">The evaluator type which should evaluate the condition alongside others of the same kind.</typeparam>
        /// <param name="configureCondition">A configuration delegate that should configure the condition builder.</param>
        /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
        public CommandBuilder AddCondition<TEval>(Action<ConditionBuilder<TEval, ICallerContext>> configureCondition)
            where TEval : ConditionEvaluator, new()
        {
            Assert.NotNull(configureCondition, nameof(configureCondition));

            var condition = new ConditionBuilder<TEval, ICallerContext>();

            configureCondition(condition);

            return AddCondition(condition);
        }

        /// <summary>
        ///     Adds a condition bound to the specified <typeparamref name="TContext"/> to the command. Conditions are used to determine if the command can be executed.
        /// </summary>
        /// <remarks>
        ///     This overload creates a new instance of the specified condition type and configures it using the provided <paramref name="configureCondition"/> delegate.
        /// </remarks>
        /// <typeparam name="TEval">The evaluator type which should evaluate the condition alongside others of the same kind.</typeparam>
        /// <typeparam name="TContext">The context type which this condition must receive in order to succeed.</typeparam>
        /// <param name="configureCondition">A configuration delegate that should configure the condition builder.</param>
        /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
        public CommandBuilder AddCondition<TEval, TContext>(Action<ConditionBuilder<TEval, TContext>> configureCondition)
            where TEval : ConditionEvaluator, new()
            where TContext : ICallerContext
        {
            Assert.NotNull(configureCondition, nameof(configureCondition));

            var condition = new ConditionBuilder<TEval, TContext>();

            configureCondition(condition);

            return AddCondition(condition);
        }

        /// <summary>
        ///     Builds the current <see cref="CommandBuilder"/> into a <see cref="CommandInfo"/> instance. 
        /// </summary>
        /// <param name="configuration">The configuration that should be used to determine the validity of the provided component.</param>
        /// <param name="parent">The parent module of this component.</param>
        /// <returns>A new instance of <see cref="CommandInfo"/> based on the configured values of this builder.</returns>
        public CommandInfo Build(ComponentConfiguration configuration, ModuleInfo? parent)
        {
            Assert.NotNull(Handler, nameof(Handler));
            Assert.Aliases(Aliases, configuration, _isNested);

            var param = Handler.Method.GetParameters();

            var hasContext = param.Length > 0 && param[0].ParameterType.IsGenericType && param[0].ParameterType.GetGenericTypeDefinition() == typeof(CommandContext<>);

            return new CommandInfo(parent, new DelegateActivator(Handler.Method, Handler.Target, hasContext), [.. Conditions.Select(x => x.Build())], [.. Aliases], hasContext, configuration);
        }

        /// <inheritdoc />
        public IComponent Build(ComponentConfiguration? configuration = null)
            => Build(configuration ?? ComponentConfiguration.Default, null);
    }
}
