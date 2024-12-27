using Commands.Conditions;
using Commands.Conversion;
using System.Text.RegularExpressions;

namespace Commands.Builders
{
    /// <summary>
    ///     A base class that represents a delegate based command, before it is built into a reflection-based executable object. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     This class is used to configure a command before it is built into a <see cref="CommandInfo"/> object. By calling the <see cref="Build(ComponentConfiguration)"/> or <see cref="Build(IEnumerable{TypeParser})"/> method, the command is built into an object that can be executed by the <see cref="IComponentTree"/>.
    /// </remarks>
    public sealed class CommandBuilder : IComponentBuilder
    {
        private readonly bool _isNested;

        private static readonly Type c_type = typeof(CommandContext<>);

        /// <inheritdoc />
        public ICollection<string> Aliases { get; set; } = [];

        /// <summary>
        ///     Gets the conditions necessary for the command to execute.
        /// </summary>
        public ICollection<IConditionBuilder> Conditions { get; set; } = [];

        /// <summary>
        ///     Gets the delegate that is executed when the command is invoked.
        /// </summary>
        public Delegate Handler { get; set; } = default!;

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
            var condition = new ConditionBuilder<TEval, TContext>();

            configureCondition(condition);

            return AddCondition(condition);
        }

        /// <inheritdoc />
        public IComponent Build(ComponentConfiguration? configuration = null)
        {
            if (Handler is null)
                throw new ArgumentNullException(nameof(Handler));

            if (!_isNested && Aliases.Count == 0)
                throw BuildException.AliasAtLeastOne();

            configuration ??= ComponentConfiguration.Default;

            var pattern = configuration.GetProperty<Regex>(ConfigurationPropertyDefinitions.NameValidationExpression);

            if (pattern != null)
            {
                foreach (var alias in Aliases)
                {
                    if (!pattern.IsMatch(alias))
                        throw BuildException.AliasConvention(alias);
                }
            }

            var param = Handler.Method.GetParameters();

            var hasContext = false;

            if (param.Length > 0 && param[0].ParameterType.IsGenericType && param[0].ParameterType.GetGenericTypeDefinition() == c_type)
                hasContext = true;

            var conditions = Conditions.Select(x => x.Build()).ToArray();

            return new CommandInfo(new DelegateActivator(Handler.Method, Handler.Target, hasContext), conditions, [.. Aliases], hasContext, configuration);
        }

        /// <summary>
        ///     Builds a searchable component from the provided configuration.
        /// </summary>
        /// <param name="converters">The typeconverters from which the current command constructs its argument converters.</param>
        /// <returns>A reflection-based container that holds information for a component ready to be executed or serves as a container for executable components.</returns>
        public IComponent Build(IEnumerable<TypeParser> converters)
            => Build(new ComponentConfiguration(converters));
    }
}
