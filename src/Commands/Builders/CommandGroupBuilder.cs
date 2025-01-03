using Commands.Conditions;

namespace Commands.Builders;

/// <summary>
///     A builder model of a group that can contain commands and sub-groups. This class cannot be inherited.
/// </summary>
/// <remarks>
///     This class is used to build a group that can contain commands and sub-groups. The group can be built using the <see cref="Build(ComponentConfiguration)"/> method, which returns a <see cref="CommandGroup"/> instance. 
///     This reflection container is not type-locked and does not have an instance. It is used to run delegate commands in a tree-like structure.
/// </remarks>
public sealed class CommandGroupBuilder : IComponentBuilder
{
    /// <inheritdoc />
    public ICollection<string> Aliases { get; set; } = [];

    /// <inheritdoc />
    public ICollection<IConditionBuilder> Conditions { get; set; } = [];

    /// <summary>
    ///     Gets or sets a collection of components that are added to the group. This collection is used to build the group into a <see cref="CommandGroup"/> object.
    /// </summary>
    public ICollection<IComponentBuilder> Components { get; set; } = [];

    /// <summary>
    ///     Creates a new instance of <see cref="CommandGroupBuilder"/>
    /// </summary>
    public CommandGroupBuilder() { }

    /// <summary>
    ///     Creates a new instance of <see cref="CommandGroupBuilder"/> with the specified name.
    /// </summary>
    /// <param name="name">The primary alias of the group.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided aliases or name are null.</exception>
    public CommandGroupBuilder(string name)
        : this(name, []) { }

    /// <summary>
    ///     Creates a new instance of <see cref="CommandGroupBuilder"/> with the specified name and aliases.
    /// </summary>
    /// <param name="name">The primary alias of the group.</param>
    /// <param name="aliases">All remaining aliases of the group.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided aliases or name are null.</exception>
    public CommandGroupBuilder(string name, IEnumerable<string> aliases)
    {
        var joined = new string[] { name }
            .Concat(aliases)
            .Distinct()
            .ToList();

        Aliases = joined;
    }

    /// <summary>
    ///     Replaces the current collection of aliases with the specified aliases. Aliases are used to identify the group in the command execution pipeline.
    /// </summary>
    /// <param name="aliases">The aliases of the group.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder WithAliases(params string[] aliases)
    {
        Assert.NotNull(aliases, nameof(aliases));

        Aliases = aliases;

        return this;
    }

    /// <summary>
    ///     Adds a new <see cref="CommandBuilder"/> to the <see cref="Components"/> collection.
    /// </summary>
    /// <param name="commandBuilder">The builder instance to add to the collection, which will be built into a <see cref="Command"/> instance that can be executed by the <see cref="IComponentTree"/>.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided builder is <see langword="null"/>.</exception>
    public CommandGroupBuilder AddCommand(CommandBuilder commandBuilder)
    {
        Assert.NotNull(commandBuilder, nameof(commandBuilder));

        Components.Add(commandBuilder);

        return this;
    }

    /// <summary>
    ///     Adds a new <see cref="CommandBuilder"/> to the <see cref="Components"/> collection.
    /// </summary>
    /// <param name="configureCommand">An action that extends the fluent API of this type to configure the command.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder AddCommand(Action<CommandBuilder> configureCommand)
    {
        Assert.NotNull(configureCommand, nameof(configureCommand));

        var commandBuilder = new CommandBuilder(true);

        configureCommand(commandBuilder);

        return AddCommand(commandBuilder);
    }

    /// <summary>
    ///     Adds a new <see cref="CommandBuilder"/> to the <see cref="Components"/> collection.
    /// </summary>
    /// <remarks>
    ///     Delegate based commands are able to access the command's scope by implementing <see cref="CommandContext{T}"/> as the first argument in the lambda signature.
    /// </remarks>
    /// <param name="executionDelegate">The delegate to execute when the root group of this object is discovered in a search operation.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder AddCommand(Delegate executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        var commandBuilder = new CommandBuilder(executionDelegate);

        return AddCommand(commandBuilder);
    }

    /// <summary>
    ///     Adds a new <see cref="CommandBuilder"/> to the <see cref="Components"/> collection.
    /// </summary>
    /// <remarks>
    ///     Delegate based commands are able to access the command's scope by implementing <see cref="CommandContext{T}"/> as the first argument in the lambda signature.
    /// </remarks>
    /// <param name="name">The name of the component.</param>
    /// <param name="executionDelegate">The delegate to execute when the provided name of this object is discovered in a search operation.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder AddCommand(string name, Delegate executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        var commandBuilder = new CommandBuilder(name, [], executionDelegate);

        return AddCommand(commandBuilder);
    }

    /// <summary>
    ///     Adds a new <see cref="CommandBuilder"/> to the <see cref="Components"/> collection.
    /// </summary>
    /// <remarks>
    ///     Delegate based commands are able to access the command's scope by implementing <see cref="CommandContext{T}"/> as the first argument in the lambda signature.
    /// </remarks>
    /// <param name="name">The name of the component.</param>
    /// <param name="executionDelegate">The delegate to execute when the provided name of this object is discovered in a search operation.</param>
    /// <param name="aliases">The aliases of the component, excluding the name.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder AddCommand(string name, Delegate executionDelegate, params string[] aliases)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        var commandBuilder = new CommandBuilder(name, aliases, executionDelegate);

        return AddCommand(commandBuilder);
    }

    /// <summary>
    ///     Adds a new <see cref="CommandGroupBuilder"/> to the <see cref="Components"/> collection.
    /// </summary>
    /// <param name="groupBuilder">The builder instance to add to the collection, which will be built into a <see cref="CommandGroup"/> instance that can contain commands to be executed by the <see cref="IComponentTree"/>.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided builder is <see langword="null"/>.</exception>
    public CommandGroupBuilder AddCommandGroup(CommandGroupBuilder groupBuilder)
    {
        Assert.NotNull(groupBuilder, nameof(groupBuilder));

        Components.Add(groupBuilder);

        return this;
    }

    /// <summary>
    ///     Adds a new <see cref="CommandGroupBuilder"/> to the <see cref="Components"/> collection.
    /// </summary>
    /// <param name="configureGroup">An action that extends the fluent API of this type to configure the group.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder AddCommandGroup(Action<CommandGroupBuilder> configureGroup)
    {
        Assert.NotNull(configureGroup, nameof(configureGroup));

        var groupBuilder = new CommandGroupBuilder();

        configureGroup(groupBuilder);

        return AddCommandGroup(groupBuilder);
    }

    /// <summary>
    ///     Replaces the current collection of conditions with the specified conditions. Conditions are used to determine if the group and subsequent commands can be executed.
    /// </summary>
    /// <param name="conditions">The conditions to add to the command execution flow.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder WithConditions(params IConditionBuilder[] conditions)
    {
        Conditions = [.. conditions];
        return this;
    }

    /// <summary>
    ///     Adds a condition to the builder. Conditions are used to determine if the group and subsequent commands can be executed.
    /// </summary>
    /// <param name="condition">The condition to add to the command execution flow.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder AddCondition(IConditionBuilder condition)
    {
        Conditions.Add(condition);
        return this;
    }

    /// <summary>
    ///     Adds a condition to the builder, which must succeed alongside other conditions with the same trigger created by this overload. Conditions are used to determine if the group and subsequent commands can be executed.
    /// </summary>
    /// <remarks>
    ///     This overload creates a new instance of the specified condition type and configures it using the provided <paramref name="configureCondition"/> delegate.
    /// </remarks>
    /// <param name="configureCondition">A configuration delegate that should configure the condition builder.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder AddCondition(Action<ConditionBuilder<ANDEvaluator, ICallerContext>> configureCondition)
        => AddCondition<ANDEvaluator, ICallerContext>(configureCondition);

    /// <summary>
    ///     Adds a condition to the builder. Conditions are used to determine if the group and subsequent commands can be executed.
    /// </summary>
    /// <remarks>
    ///     This overload creates a new instance of the specified condition type and configures it using the provided <paramref name="configureCondition"/> delegate.
    /// </remarks>
    /// <typeparam name="TEval">The evaluator type which should evaluate the condition alongside others of the same kind.</typeparam>
    /// <param name="configureCondition">A configuration delegate that should configure the condition builder.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder AddCondition<TEval>(Action<ConditionBuilder<TEval, ICallerContext>> configureCondition)
        where TEval : ConditionEvaluator, new()
    {
        var condition = new ConditionBuilder<TEval, ICallerContext>();

        configureCondition(condition);

        return AddCondition(condition);
    }

    /// <summary>
    ///     Adds a condition bound to the specified <typeparamref name="TContext"/> to the builder. Conditions are used to determine if the group and subsequent commands can be executed.
    /// </summary>
    /// <remarks>
    ///     This overload creates a new instance of the specified condition type and configures it using the provided <paramref name="configureCondition"/> delegate.
    /// </remarks>
    /// <typeparam name="TEval">The evaluator type which should evaluate the condition alongside others of the same kind.</typeparam>
    /// <typeparam name="TContext">The context type which this condition must receive in order to succeed.</typeparam>
    /// <param name="configureCondition">A configuration delegate that should configure the condition builder.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder AddCondition<TEval, TContext>(Action<ConditionBuilder<TEval, TContext>> configureCondition)
        where TEval : ConditionEvaluator, new()
        where TContext : ICallerContext
    {
        var condition = new ConditionBuilder<TEval, TContext>();

        configureCondition(condition);

        return AddCondition(condition);
    }

    /// <summary>
    ///     Builds the current <see cref="CommandGroupBuilder"/> into a <see cref="CommandGroup"/> instance.
    /// </summary>
    /// <param name="configuration">The configuration that should be used to configure the built component.</param>
    /// <param name="parent">The parent group of this component.</param>
    /// <returns>A new instance of <see cref="CommandGroup"/> based on the configured values of this builder.</returns>
    public CommandGroup Build(ComponentConfiguration configuration, CommandGroup? parent)
    {
        Assert.Aliases(Aliases, configuration, false);

        var groupInfo = new CommandGroup(parent, [.. Conditions.Select(x => x.Build())], [.. Aliases], configuration);

        foreach (var component in Components)
        {
            if (component is CommandGroupBuilder groupBuilder)
            {
                var subGroupInfo = groupBuilder.Build(configuration, groupInfo);

                groupInfo.Add(subGroupInfo);
            }
            else if (component is CommandBuilder commandBuilder)
            {
                var commandInfo = commandBuilder.Build(configuration, groupInfo);

                groupInfo.Add(commandInfo);
            }
        }

        return groupInfo;
    }

    /// <inheritdoc />
    public IComponent Build(ComponentConfiguration? configuration = null)
        => Build(configuration ?? ComponentConfiguration.Default, null);
}
