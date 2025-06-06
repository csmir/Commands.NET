using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Runtime.CompilerServices;

namespace Commands.Hosting;

/// <summary>
///     A static class containing methods for configuring a <see cref="IServiceProvider"/> with Commands.NET functionality.
/// </summary>
public static class ServiceUtils
{
    /// <summary>
    ///     Adds a <see cref="IComponentProvider"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    ///     This method will remove any existing <see cref="IComponentProvider"/> and <see cref="CommandExecutionFactory"/> from the collection before adding newly configured instances. Additionally, it configures a <see cref="IExecutionScope"/> and <see cref="IContextAccessor{TContext}"/> under scoped context.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceProvider"/> to add the configured services to.</param>
    /// <param name="configureAction"></param>
    /// <returns>The same <see cref="IServiceCollection"/> for call-chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configureAction"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddComponentProvider(this IServiceCollection services, Action<ComponentBuilderContext> configureAction)
    {
        Assert.NotNull(services, nameof(services));
        Assert.NotNull(configureAction, nameof(configureAction));

        var builder = new ComponentBuilderContext();

        configureAction(builder);

        AddComponentProvider(services, builder);

        return services;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddComponentProvider(IServiceCollection services, ComponentBuilderContext builder)
        => DefineServices(services, builder);

    #region Internals

    // A method that defines the services to be added to the service collection.
    internal static void DefineServices(IServiceCollection collection, ComponentBuilderContext builder)
    {
        Assert.NotNull(collection, nameof(collection));

        if (collection.Contains<IComponentProvider>())
        {
            // Remove the existing factory to avoid conflicts.
            collection.RemoveAll<CommandExecutionFactory>();
            collection.RemoveAll<IComponentProvider>();
            collection.RemoveAll<IExecutionScope>();
            collection.RemoveAll<IDependencyResolver>();
            collection.RemoveAll(typeof(IContextAccessor<>));
        }

        if (builder.Properties.TryGetValue("ComponentProvider", out var prop) && prop is TypeWrapper providerType)
            collection.AddSingleton(typeof(IComponentProvider), providerType.Value);

        if (builder.Properties.TryGetValue("ExecutionScope", out prop) && prop is TypeWrapper scopeType)
            collection.AddScoped(typeof(IExecutionScope), scopeType.Value);

        if (builder.Properties.TryGetValue("DependencyResolver", out prop) && prop is TypeWrapper resolverType)
            collection.AddScoped(typeof(IDependencyResolver), resolverType.Value);

        // Register the result handlers if any are defined.
        if (builder.Properties.TryGetValue("ResultHandlers", out prop) && prop is List<TypeWrapper> handlers)
            foreach (var handler in handlers)
                collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(ResultHandler), handler.Value));

        // This isn't customizable, as the logic is tightly coupled with the execution scope.
        collection.AddScoped(typeof(IContextAccessor<>), typeof(ContextAccessor<>));
        collection.AddSingleton<CommandExecutionFactory>();
    }

    #endregion
}
