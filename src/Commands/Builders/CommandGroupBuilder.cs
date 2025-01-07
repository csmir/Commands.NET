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
    public ICollection<string> Names { get; set; } = [];

    /// <inheritdoc />
    public ICollection<ExecuteCondition> Conditions { get; set; } = [];

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
    /// <exception cref="ArgumentNullException">Thrown when the provided names or name are null.</exception>
    public CommandGroupBuilder(string name)
        : this(name, []) { }

    /// <summary>
    ///     Creates a new instance of <see cref="CommandGroupBuilder"/> with the specified name and names.
    /// </summary>
    /// <param name="name">The primary alias of the group.</param>
    /// <param name="names">All remaining names of the group.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided names or name are null.</exception>
    public CommandGroupBuilder(string name, IEnumerable<string> names)
    {
        var joined = new string[] { name }
            .Concat(names)
            .Distinct()
            .ToList();

        Names = joined;
    }

    /// <summary>
    ///     Adds a name to the collection of names. Names are used to identify the group in the command execution pipeline.
    /// </summary>
    /// <param name="name">The name to add to the group.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder AddName(string name)
    {
        Assert.NotNullOrEmpty(name, nameof(name));

        Names.Add(name);

        return this;
    }

    /// <summary>
    ///     Replaces the current collection of names with the specified names. Names are used to identify the group in the command execution pipeline.
    /// </summary>
    /// <param name="names">The names of the group.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder WithNames(params string[] names)
    {
        Assert.NotNull(names, nameof(names));

        Names = names;

        return this;
    }

    /// <summary>
    ///     Adds a new <see cref="CommandBuilder"/> to the <see cref="Components"/> collection.
    /// </summary>
    /// <param name="commandBuilder">The builder instance to add to the collection, which will be built into a <see cref="Command"/> instance that can be executed by the <see cref="IExecutionProvider"/>.</param>
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
    /// <param name="names">The names of the component, excluding the name.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder AddCommand(string name, Delegate executionDelegate, params string[] names)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        var commandBuilder = new CommandBuilder(name, names, executionDelegate);

        return AddCommand(commandBuilder);
    }

    /// <summary>
    ///     Adds a new <see cref="CommandGroupBuilder"/> to the <see cref="Components"/> collection.
    /// </summary>
    /// <param name="groupBuilder">The builder instance to add to the collection, which will be built into a <see cref="CommandGroup"/> instance that can contain commands to be executed by the <see cref="IExecutionProvider"/>.</param>
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
    ///     Replaces the current collection of conditions with the specified conditions.
    /// </summary>
    /// <param name="conditions">The conditions to add to the command execution flow.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder WithConditions(params ExecuteCondition[] conditions)
    {
        Conditions = [.. conditions];
        return this;
    }

    /// <summary>
    ///     Adds a condition to the group.
    /// </summary>
    /// <param name="condition">The condition to add to the command execution flow.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder AddCondition(ExecuteCondition condition)
    {
        Assert.NotNull(condition, nameof(condition));

        Conditions.Add(condition);
        return this;
    }

    /// <summary>
    ///     Adds a condition to the group.
    /// </summary>
    /// <typeparam name="TEval">The evaluator type which should evaluate the condition alongside others of the same kind.</typeparam>
    /// <param name="executionHandler">A delegate that is responsible for executing the check.</param>
    /// <returns>The same <see cref="CommandGroupBuilder"/> for call-chaining.</returns>
    public CommandGroupBuilder AddCondition<TEval>(Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>> executionHandler)
        where TEval : ConditionEvaluator, new()
    {
        Assert.NotNull(executionHandler, nameof(executionHandler));

        return AddCondition(new DelegateExecuteCondition<TEval>(executionHandler));
    }

    /// <summary>
    ///     Builds the current <see cref="CommandGroupBuilder"/> into a <see cref="CommandGroup"/> instance.
    /// </summary>
    /// <param name="configuration">The configuration that should be used to configure the built component.</param>
    /// <param name="parent">The parent group of this component.</param>
    /// <returns>A new instance of <see cref="CommandGroup"/> based on the configured values of this builder.</returns>
    public CommandGroup Build(ComponentConfiguration configuration, CommandGroup? parent)
    {
        Assert.Names(Names, configuration, true);

        var groupInfo = new CommandGroup(parent, [.. Conditions], [.. Names], configuration);

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
