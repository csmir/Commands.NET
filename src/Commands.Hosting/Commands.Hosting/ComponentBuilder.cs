using Microsoft.Extensions.DependencyInjection.Extensions;
using System.ComponentModel;

namespace Commands.Hosting;

/// <summary>
///     A context object used to configure the component provider.
/// </summary>
public class ComponentBuilder
{
    /// <summary>
    ///     Gets a dictionary holding types required to be registered when attaching the component logistics to a service collection.
    /// </summary>
    protected Dictionary<string, TypeWrapper> ServiceDictionary { get; } = [];

    /// <summary>
    ///     Resets the component builder to its default state, using the default types for execution scope, dependency resolver, component provider, and command execution factory.
    /// </summary>
    /// <returns>The same <see cref="ComponentBuilder"/> for call-chaining.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual ComponentBuilder SetDefaults()
    {
        ServiceDictionary.Clear();

        // Reset to the default types.
        ServiceDictionary["ExecutionScope"] = new TypeWrapper(typeof(ExecutionScope));
        ServiceDictionary["DependencyResolver"] = new TypeWrapper(typeof(KeyedDependencyResolver));
        ServiceDictionary["ComponentProvider"] = new TypeWrapper(typeof(ComponentProvider));
        ServiceDictionary["CommandExecutionFactory"] = new TypeWrapper(typeof(CommandExecutionFactory));

        return this;
    }

    /// <summary>
    ///     Configures the globally available options for building components.
    /// </summary>
    /// <remarks>
    ///     This method can be called multiple times, modifying the options for each call.
    /// </remarks>
    /// <param name="configureOptions">An action that configures the creation of new components.</param>
    /// <returns>The same <see cref="ComponentBuilder"/> for call-chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureOptions"/> is <see langword="null"/>.</exception>
    public ComponentBuilder Configure(Action<ComponentOptions> configureOptions)
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
    /// <returns>The same <see cref="ComponentBuilder"/> for call-chaining.</returns>
    public ComponentBuilder AddExecutionScope<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicNestedTypes)] TScope>()
        where TScope : class, IExecutionScope
    {
        ServiceDictionary["ExecutionScope"] = new TypeWrapper(typeof(TScope));

        return this;
    }

    /// <summary>
    ///     Configures the dependency resolver type to be used by the hosted component factory to resolve dependencies for commands and modules. This resolver is used to inject services into components and commands.
    /// </summary>
    /// <remarks>
    ///     This method can be called multiple times, replacing the previously defined resolver type for a new one. If this method is not called, the default <see cref="KeyedDependencyResolver"/> will be used as the dependency resolver type.
    /// </remarks>
    /// <typeparam name="TResolver">The type implementing <see cref="IDependencyResolver"/> that should be the underlying implementation used by the hosted factory.</typeparam>
    /// <returns>The same <see cref="ComponentBuilder"/> for call-chaining.</returns>
    public ComponentBuilder AddDependencyResolver<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicNestedTypes)] TResolver>()
        where TResolver : class, IDependencyResolver
    {
        ServiceDictionary["DependencyResolver"] = new TypeWrapper(typeof(TResolver));

        return this;
    }

    /// <summary>
    ///     Configures the component provider type to be used by the hosted component factory to provide components for command execution. This provider is used to supply the factory with executable commands and modules.
    /// </summary>
    /// <remarks>
    ///     This method can be called multiple times, replacing the previously defined provider type for a new one. If this method is not called, the default <see cref="ComponentProvider"/> will be used as the component provider type.
    /// </remarks>
    /// <typeparam name="TProvider">The type implementing <see cref="IComponentProvider"/> that should be the underlying implementation used by the hosted factory.</typeparam>
    /// <returns>The same <see cref="ComponentBuilder"/> for call-chaining.</returns>
    public ComponentBuilder AddProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicNestedTypes)] TProvider>()
        where TProvider : class, IComponentProvider
    {
        ServiceDictionary["ComponentProvider"] = new TypeWrapper(typeof(TProvider));

        return this;
    }

    /// <summary>
    ///     Configures the command execution factory type to be used by the hosted component factory to manage command execution. This factory is responsible for creating and managing the execution scope, resolving dependencies, and executing commands.
    /// </summary>
    /// <typeparam name="TFactory">The type implementing <see cref="ICommandExecutionFactory"/> that should be the underlying implementation for the hosted factory.</typeparam>
    /// <returns>The same <see cref="ComponentBuilder"/> for call-chaining.</returns>
    public ComponentBuilder AddFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicNestedTypes)] TFactory>()
        where TFactory : class, ICommandExecutionFactory
    {
        ServiceDictionary["CommandExecutionFactory"] = new TypeWrapper(typeof(TFactory));

        return this;
    }

    #region Internals

    // A method that defines the services to be added to the service collection.
    internal void DefineServices(IServiceCollection collection)
    {
        Assert.NotNull(collection, nameof(collection));

        if (collection.Contains<ICommandExecutionFactory>())
        {
            // Remove the existing factory to avoid conflicts.
            collection.RemoveAll<ICommandExecutionFactory>();
            collection.RemoveAll<IComponentProvider>();
            collection.RemoveAll<IExecutionScope>();
            collection.RemoveAll<IDependencyResolver>();
            collection.RemoveAll(typeof(IContextAccessor<>));
        }

        collection.AddSingleton(typeof(ICommandExecutionFactory), ServiceDictionary["CommandExecutionFactory"].Value);
        collection.AddSingleton(typeof(IComponentProvider), ServiceDictionary["ComponentProvider"].Value);

        collection.AddScoped(typeof(IDependencyResolver), ServiceDictionary["DependencyResolver"].Value);
        collection.AddScoped(typeof(IExecutionScope), ServiceDictionary["ExecutionScope"].Value);

        // This isn't customizable, as the logic is tightly coupled with the execution scope.
        collection.AddScoped(typeof(IContextAccessor<>), typeof(ContextAccessor<>));
    }

    #endregion
}
