namespace Commands;

public sealed class ComponentManagerProperties
{
    private readonly List<DynamicType> _dynamicTypes;

    private readonly List<ComponentProperties> _components;
    private readonly List<ResultHandlerProperties> _handlers;

    private ComponentConfigurationProperties? _configuration;

    public ComponentManagerProperties()
    {
        _components = [];
        _handlers = [];

        _configuration = null;
    }

    public ComponentManagerProperties Component(ComponentProperties component)
    {
        Assert.NotNull(component, nameof(component));

        _components.Add(component);

        return this;
    }

    public ComponentManagerProperties Type(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
# endif
        Type componentType)
    {
        Assert.NotNull(componentType, nameof(componentType));

        _dynamicTypes.Add(componentType);

        return this;
    }

    public ComponentManagerProperties Type<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        TComponent>()
        where TComponent : CommandModule
    {
        return Type(new DynamicType(typeof(TComponent)));
    }

#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("AotAnalysis", "IL2072", Justification = "The types are supplied from user-facing implementation, it is up to the user to ensure that these types are available in AOT context.")]
#endif
    public ComponentManagerProperties Types(params Type[] componentTypes)
    {
        foreach (var componentType in componentTypes)
            Type(componentType);

        return this;
    }

    public ComponentManagerProperties Components(params ComponentProperties[] componentDefinitions)
    {
        foreach (var component in componentDefinitions)
            Component(component);

        return this;
    }

    public ComponentManagerProperties Handler(ResultHandlerProperties handler)
    {
        Assert.NotNull(handler, nameof(handler));

        _handlers.Add(handler);

        return this;
    }

    public ComponentManagerProperties Handlers(params ResultHandlerProperties[] handlers)
    {
        foreach (var handler in handlers)
            Handler(handler);

        return this;
    }

    public ComponentManagerProperties Configuration(ComponentConfigurationProperties configuration)
    {
        Assert.NotNull(configuration, nameof(configuration));

        _configuration = configuration;

        return this;
    }

    public ComponentManager ToManager()
    {
        _configuration ??= ComponentConfigurationProperties.Default;

        var configuration = _configuration.ToConfiguration();

        var componentsToAdd = _components.Select(component => component.ToComponent(configuration: configuration));

        var handlers = _handlers.Select(handler => handler.ToHandler());

        var manager = new ComponentManager(handlers);

        manager.AddRange([.. componentsToAdd]);

        return manager;
    }
}
