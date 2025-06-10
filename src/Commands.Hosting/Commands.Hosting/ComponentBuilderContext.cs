namespace Commands.Hosting;

/// <summary>
///     A context object used to configure the component provider.
/// </summary>
public class ComponentBuilderContext
{
    internal Dictionary<string, object?> Properties { get; } = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="ComponentBuilderContext"/> class, setting up the default services required for component execution.
    /// </summary>
    public ComponentBuilderContext()
    {
        // Initialize the default service properties with their respective types.
        Properties[nameof(IExecutionScope)] = typeof(ExecutionScope);
        Properties[nameof(IDependencyResolver)] = typeof(KeyedDependencyResolver);
        Properties[nameof(IComponentProvider)] = typeof(ComponentProvider);

        // Initialize the range of result handlers. These will hold the types of handlers that will be added on post-configure; These are hashsets to avoid duplicates.
        Properties[nameof(IResultHandler)] = new HashSet<Type>();
    }

    /// <summary>
    ///     Configures the globally available options for building components.
    /// </summary>
    /// <remarks>
    ///     This method can be called multiple times, modifying the options for each call.
    /// </remarks>
    /// <param name="configureOptions">An action that configures the creation of new components.</param>
    /// <returns>The same <see cref="ComponentBuilderContext"/> for call-chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureOptions"/> is <see langword="null"/>.</exception>
    public ComponentBuilderContext ConfigureOptions(Action<ComponentOptions> configureOptions)
    {
        Assert.NotNull(configureOptions, nameof(configureOptions));

        configureOptions(ComponentOptions.Default);

        return this;
    }

    /// <summary>
    ///     Configures the execution scope type to be used by the hosted component factory to manage the state of execution. This scope is used to manage the lifetime of the command execution and its related services.
    /// </summary>
    /// <remarks>
    ///     This method can be called multiple times, replacing the previously defined scope type for a new one. If this method is not called, the default <see cref="ExecutionScope"/> will be used as the execution scope type.
    /// </remarks>
    /// <typeparam name="TScope">The type implementing <see cref="IExecutionScope"/> that should be the underlying implementation used by the hosted factory.</typeparam>
    /// <returns>The same <see cref="ComponentBuilderContext"/> for call-chaining.</returns>
    public ComponentBuilderContext WithExecutionScope<TScope>()
        where TScope : class, IExecutionScope
    {
        Properties[nameof(IExecutionScope)] = typeof(TScope);

        return this;
    }

    /// <summary>
    ///     Configures the dependency resolver type to be used by the hosted component factory to resolve dependencies for commands and modules. This resolver is used to inject services into components and commands.
    /// </summary>
    /// <remarks>
    ///     This method can be called multiple times, replacing the previously defined resolver type for a new one. If this method is not called, the default <see cref="KeyedDependencyResolver"/> will be used as the dependency resolver type.
    /// </remarks>
    /// <typeparam name="TResolver">The type implementing <see cref="IDependencyResolver"/> that should be the underlying implementation used by the hosted factory.</typeparam>
    /// <returns>The same <see cref="ComponentBuilderContext"/> for call-chaining.</returns>
    public ComponentBuilderContext WithDependencyResolver<TResolver>()
        where TResolver : class, IDependencyResolver
    {
        Properties[nameof(IDependencyResolver)] = typeof(TResolver);

        return this;
    }

    /// <summary>
    ///     Configures the component provider type to be used by the hosted component factory to provide components for command execution. This provider is used to supply the factory with executable commands and modules.
    /// </summary>
    /// <remarks>
    ///     This method can be called multiple times, replacing the previously defined provider type for a new one. If this method is not called, the default <see cref="ComponentProvider"/> will be used as the component provider type.
    /// </remarks>
    /// <typeparam name="TProvider">The type implementing <see cref="IComponentProvider"/> that should be the underlying implementation used by the hosted factory.</typeparam>
    /// <returns>The same <see cref="ComponentBuilderContext"/> for call-chaining.</returns>
    public ComponentBuilderContext WithComponentProvider<TProvider>()
        where TProvider : class, IComponentProvider
    {
        Properties[nameof(IComponentProvider)] = typeof(TProvider);

        return this;
    }

    /// <summary>
    ///     Adds a result handler type to the component builder. Result handlers are used to process the results of command execution, allowing for custom handling of success and failure cases.
    /// </summary>
    /// <typeparam name="THandler">The type implementing <see cref="IResultHandler"/> that should be an enumerated implementation to handle command results.</typeparam>
    /// <returns>The same <see cref="ComponentBuilderContext"/> for call-chaining.</returns>
    public ComponentBuilderContext AddResultHandler<THandler>()
        where THandler : IResultHandler
    {
        if (!TryGetProperty<HashSet<Type>>(nameof(IResultHandler), out var handlersProperty))
        {
            // If the property is not found, create a new HashSet and add it to the properties.
            handlersProperty = [];

            Properties[nameof(IResultHandler)] = handlersProperty;
        }

        handlersProperty.Add(typeof(THandler));

        return this;
    }

    #region Internals

    internal bool TryGetProperty<T>(string key, [NotNullWhen(true)] out T? property)
    {
        Assert.NotNull(key, nameof(key));

        if (Properties.TryGetValue(key, out var value) && value is T typed)
        {
            property = typed;
            return true;
        }

        property = default;
        return false;
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    internal Type GetTypeProperty(string key, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type defaultValue)
    {
        if (Properties.TryGetValue(key, out var value) && value is Type type)
            return type;

        return defaultValue;
    }

    #endregion
}