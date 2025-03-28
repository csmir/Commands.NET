﻿namespace Commands;

/// <summary>
///     A set of properties for a component manager.
/// </summary>
public sealed class ComponentCollectionProperties
{
    private readonly List<DynamicType> _dynamicTypes;

    private readonly List<IComponentProperties> _components;
    private readonly List<IResultHandlerProperties> _handlers;

    private ComponentConfigurationProperties? _configuration;

    /// <summary>
    ///     Creates a new instance of <see cref="ComponentCollectionProperties"/>.
    /// </summary>
    public ComponentCollectionProperties()
    {
        _dynamicTypes = [];
        _components = [];
        _handlers = [];

        _configuration = null;
    }

    /// <summary>
    ///     Adds a type to the component manager.
    /// </summary>
    /// <remarks>
    ///     Types are evaluated whether they implement <see cref="CommandModule"/>, are not abstract, and have no open generic parameters when the manager is created. Any added types that do not match this constraint are ignored.
    /// </remarks>
    /// <param name="type">The type to add. If the type is already added, it is ignored.</param>
    /// <returns>The same <see cref="ComponentCollectionProperties"/> for call-chaining.</returns>
    public ComponentCollectionProperties Type(
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
    ///     Adds a type to the component manager.
    /// </summary>
    /// <typeparam name="T">The type definition to add. If the type is already added, it is ignored.</typeparam>
    /// <returns>The same <see cref="ComponentCollectionProperties"/> for call-chaining.</returns>
    public ComponentCollectionProperties Type<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
    T>()
        where T : CommandModule
    {
        return Type(new DynamicType(typeof(T)));
    }

    /// <summary>
    ///     Adds multiple types to the component manager.
    /// </summary>
    /// <param name="types">The types to add. If any type is already added, it is ignored.</param>
    /// <returns>The same <see cref="ComponentCollectionProperties"/> for call-chaining.</returns>
#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("AotAnalysis", "IL2072", Justification = "The types are supplied from user-facing implementation, it is up to the user to ensure that these types are available in AOT context.")]
#endif
    public ComponentCollectionProperties Types(params Type[] types)
    {
        foreach (var componentType in types)
            Type(componentType);

        return this;
    }

    /// <summary>
    ///     Adds a component to the component manager.
    /// </summary>
    /// <remarks>
    ///     Commands added to the manager must have at least one name. Groups that are added are not required to have a name.
    /// </remarks>
    /// <param name="component">The component to add.</param>
    /// <returns>The same <see cref="ComponentCollectionProperties"/> for call-chaining.</returns>
    public ComponentCollectionProperties Component(IComponentProperties component)
    {
        Assert.NotNull(component, nameof(component));

        _components.Add(component);

        return this;
    }

    /// <summary>
    ///     Adds multiple components to the component manager.
    /// </summary>
    /// <remarks>
    ///     Commands added to the manager must have at least one name. Groups that are added are not required to have a name.
    /// </remarks>
    /// <param name="components">The components to add.</param>
    /// <returns>The same <see cref="ComponentCollectionProperties"/> for call-chaining.</returns>
    public ComponentCollectionProperties Components(params IComponentProperties[] components)
    {
        foreach (var component in components)
            Component(component);

        return this;
    }

    /// <summary>
    ///     Adds a result handler to the component manager.
    /// </summary>
    /// <param name="handler">The handler to add.</param>
    /// <returns>The same <see cref="ComponentCollectionProperties"/> for call-chaining.</returns>
    public ComponentCollectionProperties Handler(IResultHandlerProperties handler)
    {
        Assert.NotNull(handler, nameof(handler));

        _handlers.Add(handler);

        return this;
    }

    /// <summary>
    ///     Adds a result handler to the component manager.
    /// </summary>
    /// <param name="handler">The handler to add.</param>
    /// <returns>The same <see cref="ComponentCollectionProperties"/> for call-chaining.</returns>
    public ComponentCollectionProperties Handler(ResultHandler handler)
        => Handler(new ResultHandlerProperties(handler));

    /// <summary>
    ///     Adds multiple result handlers to the component manager.
    /// </summary>
    /// <param name="handlers">The handlers to add.</param>
    /// <returns>The same <see cref="ComponentCollectionProperties"/> for call-chaining.</returns>
    public ComponentCollectionProperties Handlers(params IResultHandlerProperties[] handlers)
    {
        foreach (var handler in handlers)
            Handler(handler);

        return this;
    }

    /// <summary>
    ///     Adds multiple result handlers to the component manager.
    /// </summary>
    /// <param name="handlers">The handlers to add.</param>
    /// <returns>The same <see cref="ComponentCollectionProperties"/> for call-chaining.</returns>
    public ComponentCollectionProperties Handlers(params ResultHandler[] handlers)
    {
        foreach (var handler in handlers)
            Handler(new ResultHandlerProperties(handler));

        return this;
    }

    /// <summary>
    ///     Sets the configuration for the component manager.
    /// </summary>
    /// <param name="configuration">The configuration which should configure defined components that are to be built for this manager.</param>
    /// <returns>The same <see cref="ComponentCollectionProperties"/> for call-chaining.</returns>
    public ComponentCollectionProperties Configuration(ComponentConfigurationProperties configuration)
    {
        Assert.NotNull(configuration, nameof(configuration));

        _configuration = configuration;

        return this;
    }

    /// <summary>
    ///     Converts this set of properties to a new instance of <see cref="ComponentCollection"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="ComponentCollection"/>.</returns>
    public ComponentCollection Create()
    {
        _configuration ??= ComponentConfigurationProperties.Default;

        var configuration = _configuration.Create();

        var manager = new ComponentCollection(configuration, [.. _handlers.Select(handler => handler.Create())]);

        manager.AddRange(_components.Select(component => component.Create(configuration: configuration)));
        manager.AddRange(ComponentUtilities.GetComponents(configuration, _dynamicTypes, null, false));

        return manager;
    }
}
