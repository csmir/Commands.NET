namespace Commands.Builders;

/// <inheritdoc cref="IManagerBuilder"/>
public sealed class ComponentManagerBuilder : IManagerBuilder
{
    /// <inheritdoc />
    public IConfigurationBuilder Configuration { get; set; } = new ComponentConfigurationBuilder();

    /// <inheritdoc />
    public ICollection<IComponentBuilder> Components { get; set; } = [];

    /// <inheritdoc />
    public ICollection<ResultHandler> Handlers { get; set; } = [];

    /// <inheritdoc />
    public ICollection<DynamicType> Types { get; set; } = [];

    /// <inheritdoc />
    public IManagerBuilder AddCommand(CommandBuilder commandBuilder)
    {
        Assert.NotNull(commandBuilder, nameof(commandBuilder));

        Components.Add(commandBuilder);

        return this;
    }

    /// <inheritdoc />
    public IManagerBuilder AddCommand(Action<CommandBuilder> configureCommand)
    {
        var commandBuilder = new CommandBuilder();

        configureCommand(commandBuilder);

        return AddCommand(commandBuilder);
    }

    /// <inheritdoc />
    public IManagerBuilder AddCommand(string name, Delegate executionDelegate)
    {
        var commandBuilder = new CommandBuilder(name, [], executionDelegate);

        return AddCommand(commandBuilder);
    }

    /// <inheritdoc />
    public IManagerBuilder AddCommand(string name, Delegate executionDelegate, params string[] names)
    {
        var commandBuilder = new CommandBuilder(name, names, executionDelegate);

        return AddCommand(commandBuilder);
    }

    /// <inheritdoc />
    public IManagerBuilder AddCommandGroup(CommandGroupBuilder groupBuilder)
    {
        Assert.NotNull(groupBuilder, nameof(groupBuilder));

        Components.Add(groupBuilder);

        return this;
    }

    /// <inheritdoc />
    public IManagerBuilder AddCommandGroup(Action<CommandGroupBuilder> configureGroup)
    {
        var groupBuilder = new CommandGroupBuilder();

        configureGroup(groupBuilder);

        return AddCommandGroup(groupBuilder);
    }

    /// <inheritdoc />
    public IManagerBuilder AddType(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
# endif
        Type groupType)
    {
        Assert.NotNull(groupType, nameof(groupType));

        if (Types.Contains(groupType))
            return this;

        Types.Add(groupType);

        return this;
    }

    /// <inheritdoc />
    public IManagerBuilder AddType<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
    T>()
        where T : class
        => AddType(typeof(T));

    /// <inheritdoc />
#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("AotAnalysis", "IL2072", Justification = "The types are supplied from user-facing implementation, it is up to the user to ensure that these types are available in AOT context.")]
#endif
    public IManagerBuilder WithTypes(params Type[] types)
    {
        // We cannot reassign the collection, because we need AddType to infer DynamicallyAccessedMemberTypes.All
        Types = [];

        foreach (var type in types)
            AddType(type);

        return this;
    }

    /// <inheritdoc />
    public IManagerBuilder AddResultHandler(Action<ICallerContext, IExecuteResult, IServiceProvider> resultAction)
    {
        Assert.NotNull(resultAction, nameof(resultAction));
        Handlers.Add(new DelegateResultHandler(resultAction));
        return this;
    }

    /// <inheritdoc />
    public IManagerBuilder AddResultHandler(Func<ICallerContext, IExecuteResult, IServiceProvider, ValueTask> resultAction)
    {
        Assert.NotNull(resultAction, nameof(resultAction));

        Handlers.Add(new DelegateResultHandler(resultAction));

        return this;
    }

    /// <inheritdoc />
    public IManagerBuilder AddResultHandler<T>(Action<T, IExecuteResult, IServiceProvider> resultAction)
        where T : class, ICallerContext
    {
        Assert.NotNull(resultAction, nameof(resultAction));
        Handlers.Add(new DelegateResultHandler<T>(resultAction));
        return this;
    }

    /// <inheritdoc />
    public IManagerBuilder AddResultHandler<T>(Func<T, IExecuteResult, IServiceProvider, ValueTask> resultAction)
        where T : class, ICallerContext
    {
        Assert.NotNull(resultAction, nameof(resultAction));

        Handlers.Add(new DelegateResultHandler<T>(resultAction));

        return this;
    }

    /// <inheritdoc />
    public IManagerBuilder AddResultHandler(ResultHandler resolver)
    {
        Assert.NotNull(resolver, nameof(resolver));

        Handlers.Add(resolver);

        return this;
    }

    /// <inheritdoc />
    public IManagerBuilder ConfigureComponents(Action<IConfigurationBuilder> configure)
    {
        Assert.NotNull(configure, nameof(configure));

        Configuration ??= new ComponentConfigurationBuilder();

        configure(Configuration);

        return this;
    }

    /// <inheritdoc />
    public ComponentManager Build()
    {
        var configuration = Configuration.Build();

        var components = configuration.BuildGroups([.. Types], null, false)
            .Concat(Components.Select(x => x.Build(configuration)));

        return new(components, [.. Handlers]);
    }
}
