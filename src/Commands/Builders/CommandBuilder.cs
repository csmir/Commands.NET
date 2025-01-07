namespace Commands.Builders;

/// <summary>
///     A builder model that represents the construction of a delegate based command. This class cannot be inherited.
/// </summary>
/// <remarks>
///     This class is used to configure a command before it is built into a <see cref="Command"/> object. 
///     By calling <see cref="Build(ComponentConfiguration)"/>, the command is built into an object that can be executed by the <see cref="IExecutionProvider"/>.
/// </remarks>
public sealed class CommandBuilder : IComponentBuilder
{
    private readonly bool _isNested;

    /// <inheritdoc />
    public ICollection<string> Names { get; set; } = [];

    /// <inheritdoc />
    public ICollection<ExecuteCondition> Conditions { get; set; } = [];

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
    ///     Creates a new instance of <see cref="CommandBuilder"/> with the specified name, names, and delegate.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    /// <param name="names">The names of the command, excluding the name.</param>
    /// <param name="executeDelegate">The delegate used to execute the command.</param>
    public CommandBuilder(string name, IEnumerable<string> names, Delegate executeDelegate)
    {
        var joined = new string[] { name }
            .Concat(names)
            .Distinct()
            .ToArray();

        Names = joined;
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
    ///     Adds a name to the collection of names. Names are used to identify the command in the command execution pipeline.
    /// </summary>
    /// <param name="name">The name to add to the command.</param>
    /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
    public CommandBuilder AddName(string name)
    {
        Assert.NotNullOrEmpty(name, nameof(name));

        Names.Add(name);

        return this;
    }

    /// <summary>
    ///     Replaces the current collection of names with the specified names. Names are used to identify the command in the command execution pipeline.
    /// </summary>
    /// <param name="names">The names of the command.</param>
    /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
    public CommandBuilder WithNames(params string[] names)
    {
        Names = names;

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
    ///     Replaces the current collection of conditions with the specified conditions.
    /// </summary>
    /// <param name="conditions">The conditions to add to the command execution flow.</param>
    /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
    public CommandBuilder WithConditions(params ExecuteCondition[] conditions)
    {
        Conditions = [.. conditions];
        return this;
    }

    /// <summary>
    ///     Adds a condition to the command.
    /// </summary>
    /// <param name="condition">The condition to add to the command execution flow.</param>
    /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
    public CommandBuilder AddCondition(ExecuteCondition condition)
    {
        Assert.NotNull(condition, nameof(condition));

        Conditions.Add(condition);
        return this;
    }

    /// <summary>
    ///     Adds a condition to the command.
    /// </summary>
    /// <typeparam name="TEval">The evaluator type which should evaluate the condition alongside others of the same kind.</typeparam>
    /// <param name="executionHandler">A delegate that is responsible for executing the check.</param>
    /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
    public CommandBuilder AddCondition<TEval>(Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>> executionHandler)
        where TEval : ConditionEvaluator, new()
    {
        Assert.NotNull(executionHandler, nameof(executionHandler));

        return AddCondition(new DelegateExecuteCondition<TEval>(executionHandler));
    }

    /// <summary>
    ///     Builds the current <see cref="CommandBuilder"/> into a <see cref="Command"/> instance. 
    /// </summary>
    /// <param name="configuration">The configuration that should be used to configure the built component.</param>
    /// <param name="parent">The parent module of this component.</param>
    /// <returns>A new instance of <see cref="Command"/> based on the configured values of this builder.</returns>
    public Command Build(ComponentConfiguration configuration, CommandGroup? parent)
    {
        Assert.NotNull(Handler, nameof(Handler));
        Assert.Names(Names, configuration, _isNested);

        var hasContext = Handler.Method.HasContext();

        return new Command(parent, new CommandDelegateActivator(Handler.Method, Handler.Target, hasContext), [.. Conditions], [.. Names], hasContext, configuration);
    }

    /// <inheritdoc />
    public IComponent Build(ComponentConfiguration? configuration = null)
        => Build(configuration ?? ComponentConfiguration.Default, null);
}
