namespace Commands;

/// <summary>
///     A set of properties for a component provider.
/// </summary>
public sealed class ComponentProviderProperties
{
    private readonly List<DynamicType> _dynamicTypes;

    private readonly List<IComponentProperties> _components;
    private readonly List<IResultHandlerProperties> _handlers;

    private ComponentConfigurationProperties? _configuration;

    /// <summary>
    ///     Creates a new instance of <see cref="ComponentProviderProperties"/>.
    /// </summary>
    public ComponentProviderProperties()
    {
        _dynamicTypes = [];
        _components = [];
        _handlers = [];

        _configuration = null;
    }

    /// <summary>
    ///     Adds a type to the component provider. This operation can include non-command module types, but they will be ignored when the provider is created.
    /// </summary>
    /// <remarks>
    ///     Types are evaluated whether they implement <see cref="CommandModule"/>, are not abstract, and have no open generic parameters when the provider is created. Any added types that do not match this constraint are ignored.
    /// </remarks>
    /// <param name="type">The type to add. If the type is already added, it is ignored.</param>
    /// <returns>The same <see cref="ComponentProviderProperties"/> for call-chaining.</returns>
    public ComponentProviderProperties AddComponentType(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
# endif
        Type type)
    {
        Assert.NotNull(type, nameof(type));

        if (_dynamicTypes.Contains(type))
            return this;

        _dynamicTypes.Add(type);

        return this;
    }

    /// <summary>
    ///     Adds a type to the component provider. This operation can include non-command module types, but they will be ignored when the provider is created.
    /// </summary>
    /// <typeparam name="T">The type definition to add. If the type is already added, it is ignored.</typeparam>
    /// <returns>The same <see cref="ComponentProviderProperties"/> for call-chaining.</returns>
    public ComponentProviderProperties AddComponentType<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
    T>()
        where T : CommandModule
    {
        return AddComponentType(new DynamicType(typeof(T)));
    }

    /// <summary>
    ///     Adds multiple types to the component provider. This operation can include non-command module types, but they will be ignored when the provider is created.
    /// </summary>
    /// <remarks>
    ///     When <see cref="ToProvider"/> is called on the properties, all types added to the properties are checked if they implement <see cref="CommandModule"/> or <see cref="CommandModule{T}"/>. 
    ///     If any provided type does not implement said base type, it is ignored.
    /// </remarks>
    /// <param name="types">The types to add. If any type is already added, it is ignored.</param>
    /// <returns>The same <see cref="ComponentProviderProperties"/> for call-chaining.</returns>
#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("AotAnalysis", "IL2072", Justification = "The types are supplied from user-facing implementation, it is up to the user to ensure that these types are available in AOT context.")]
#endif
    public ComponentProviderProperties AddComponentTypes(params Type[] types)
    {
        foreach (var componentType in types)
            AddComponentType(componentType);

        return this;
    }

    /// <summary>
    ///     Adds a component to the component provider.
    /// </summary>
    /// <remarks>
    ///     Commands added to the provider must have at least one name. Groups that are added are not required to have a name.
    /// </remarks>
    /// <param name="component">The component to add.</param>
    /// <returns>The same <see cref="ComponentProviderProperties"/> for call-chaining.</returns>
    public ComponentProviderProperties AddComponent(IComponentProperties component)
    {
        Assert.NotNull(component, nameof(component));

        _components.Add(component);

        return this;
    }

    /// <summary>
    ///     Adds multiple components to the component provider.
    /// </summary>
    /// <remarks>
    ///     Commands added to the provider must have at least one name. Groups that are added are not required to have a name.
    /// </remarks>
    /// <param name="components">The components to add.</param>
    /// <returns>The same <see cref="ComponentProviderProperties"/> for call-chaining.</returns>
    public ComponentProviderProperties AddComponents(params IComponentProperties[] components)
    {
        foreach (var component in components)
            AddComponent(component);

        return this;
    }

    /// <summary>
    ///     Adds a result handler to the component provider.
    /// </summary>
    /// <param name="handler">The handler to add.</param>
    /// <returns>The same <see cref="ComponentProviderProperties"/> for call-chaining.</returns>
    public ComponentProviderProperties AddResultHandler(IResultHandlerProperties handler)
    {
        Assert.NotNull(handler, nameof(handler));

        _handlers.Add(handler);

        return this;
    }

    /// <summary>
    ///     Adds a result handler to the component provider.
    /// </summary>
    /// <param name="handler">The handler to add.</param>
    /// <returns>The same <see cref="ComponentProviderProperties"/> for call-chaining.</returns>
    public ComponentProviderProperties AddResultHandler(ResultHandler handler)
        => AddResultHandler(new ResultHandlerProperties(handler));

    /// <summary>
    ///     Adds a result handler to the component provider.
    /// </summary>
    /// <typeparam name="T">The context type for the handler to handle.</typeparam>
    /// <param name="executionDelegate">The delegate that is executed when the result of command execution is yielded.</param>
    /// <returns>The same <see cref="ComponentProviderProperties"/> for call-chaining.</returns>
    public ComponentProviderProperties AddResultHandler<T>(Action<T, Exception, IServiceProvider> executionDelegate)
        where T : class, ICallerContext
        => AddResultHandler(new ResultHandlerProperties<T>().AddDelegate(executionDelegate));

    /// <summary>
    ///     Adds a result handler to the component provider.
    /// </summary>
    /// <typeparam name="T">The context type for the handler to handle.</typeparam>
    /// <param name="executionDelegate">The delegate that is executed when the result of command execution is yielded.</param>
    /// <returns>The same <see cref="ComponentProviderProperties"/> for call-chaining.</returns>
    public ComponentProviderProperties AddResultHandler<T>(Func<T, Exception, IServiceProvider, ValueTask> executionDelegate)
        where T : class, ICallerContext
        => AddResultHandler(new ResultHandlerProperties<T>().AddDelegate(executionDelegate));

    /// <summary>
    ///     Adds multiple result handlers to the component provider.
    /// </summary>
    /// <param name="handlers">The handlers to add.</param>
    /// <returns>The same <see cref="ComponentProviderProperties"/> for call-chaining.</returns>
    public ComponentProviderProperties AddResultHandlers(params IResultHandlerProperties[] handlers)
    {
        foreach (var handler in handlers)
            AddResultHandler(handler);

        return this;
    }

    /// <summary>
    ///     Adds multiple result handlers to the component provider.
    /// </summary>
    /// <param name="handlers">The handlers to add.</param>
    /// <returns>The same <see cref="ComponentProviderProperties"/> for call-chaining.</returns>
    public ComponentProviderProperties AddResultHandlers(params ResultHandler[] handlers)
    {
        foreach (var handler in handlers)
            AddResultHandler(new ResultHandlerProperties(handler));

        return this;
    }

    /// <summary>
    ///     Sets the configuration for the component provider.
    /// </summary>
    /// <param name="configuration">The configuration which should configure defined components that are to be built for this provider.</param>
    /// <returns>The same <see cref="ComponentProviderProperties"/> for call-chaining.</returns>
    public ComponentProviderProperties WithConfiguration(ComponentConfigurationProperties configuration)
    {
        Assert.NotNull(configuration, nameof(configuration));

        _configuration = configuration;

        return this;
    }

    /// <summary>
    ///     Sets the configuration for the component provider.
    /// </summary>
    /// <param name="configure">An action that configures a newly created instance of <see cref="ComponentConfigurationProperties"/> to be built for this provider.</param>
    /// <returns>The same <see cref="ComponentProviderProperties"/> for call-chaining.</returns>
    public ComponentProviderProperties WithConfiguration(Action<ComponentConfigurationProperties> configure)
    {
        Assert.NotNull(configure, nameof(configure));

        var configuration = new ComponentConfigurationProperties();
        configure(configuration);

        _configuration = configuration;

        return this;
    }

    /// <summary>
    ///     Converts this set of properties to a new instance of <see cref="ComponentProvider"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="ComponentProvider"/>.</returns>
    public ComponentProvider ToProvider()
    {
        _configuration ??= ComponentConfigurationProperties.Default;

        var configuration = _configuration.ToConfiguration();

        var provider = new ComponentProvider(configuration, [.. _handlers.Select(handler => handler.ToHandler())]);

        provider.AddRange(_components.Select(component => component.ToComponent(configuration: configuration)));
        provider.AddRange(ComponentUtilities.GetComponents(configuration, _dynamicTypes, null, false));

        return provider;
    }
}
