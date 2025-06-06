using Microsoft.Extensions.DependencyInjection.Extensions;
using System.ComponentModel;

namespace Commands.Hosting;

/// <summary>
///     A context object used to configure the component provider.
/// </summary>
public class ComponentBuilderContext
{
    /// <summary>
    ///     Gets a dictionary holding types required to be registered when attaching the component logistics to a service collection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Dictionary<string, object?> Properties { get; } = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="ComponentBuilderContext"/> class, setting up the default services required for component execution.
    /// </summary>
    public ComponentBuilderContext()
    {
        // Initialize the default service properties with their respective types.
        Properties["ExecutionScope"] = new TypeWrapper(typeof(ExecutionScope));
        Properties["DependencyResolver"] = new TypeWrapper(typeof(KeyedDependencyResolver));
        Properties["ComponentProvider"] = new TypeWrapper(typeof(ComponentProvider));
        Properties["CommandExecutionFactory"] = new TypeWrapper(typeof(CommandExecutionFactory));
        Properties["ResultHandlers"] = new List<TypeWrapper>();
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
    public ComponentBuilderContext WithExecutionScope<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicNestedTypes)] TScope>()
        where TScope : class, IExecutionScope
    {
        Properties["ExecutionScope"] = new TypeWrapper(typeof(TScope));

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
    public ComponentBuilderContext WithDependencyResolver<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicNestedTypes)] TResolver>()
        where TResolver : class, IDependencyResolver
    {
        Properties["DependencyResolver"] = new TypeWrapper(typeof(TResolver));

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
    public ComponentBuilderContext WithComponentProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicNestedTypes)] TProvider>()
        where TProvider : class, IComponentProvider
    {
        Properties["ComponentProvider"] = new TypeWrapper(typeof(TProvider));

        return this;
    }

    /// <summary>
    ///     Configures the command execution factory type to be used by the hosted component factory to manage command execution. This factory is responsible for creating and managing the execution scope, resolving dependencies, and executing commands.
    /// </summary>
    /// <typeparam name="TFactory">The type implementing <see cref="ICommandExecutionFactory"/> that should be the underlying implementation for the hosted factory.</typeparam>
    /// <returns>The same <see cref="ComponentBuilderContext"/> for call-chaining.</returns>
    public ComponentBuilderContext WithExecutionFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicNestedTypes)] TFactory>()
        where TFactory : class, ICommandExecutionFactory
    {
        Properties["CommandExecutionFactory"] = new TypeWrapper(typeof(TFactory));

        return this;
    }

    /// <summary>
    ///     Adds a result handler type to the component builder. Result handlers are used to process the results of command execution, allowing for custom handling of success and failure cases.
    /// </summary>
    /// <typeparam name="THandler">The type implementing <see cref="ResultHandler"/> that should be an enumerated implementation to handle command results.</typeparam>
    /// <returns>The same <see cref="ComponentBuilderContext"/> for call-chaining.</returns>
    public ComponentBuilderContext AddResultHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicNestedTypes)] THandler>()
        where THandler : ResultHandler
    {
        if (!Properties.TryGetValue("ResultHandlers", out var handlers) || handlers is not List<TypeWrapper> handlerContainer)
        {
            handlerContainer = [];
            
            Properties["ResultHandlers"] = handlerContainer;
        }

        handlerContainer.Add(new TypeWrapper(typeof(THandler)));

        return this;
    }
}