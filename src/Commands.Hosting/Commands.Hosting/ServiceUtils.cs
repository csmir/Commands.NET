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
    ///     This method will try to add any resources not yet added to the service collection.
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
        => TryAddServices(services, builder);

    #region Internals

    // A method that defines the services to be added to the service collection.
    internal static void TryAddServices(IServiceCollection collection, ComponentBuilderContext builder)
    {
        if (builder.Properties.TryGetValue(nameof(IComponentProvider), out var prop) && prop is TypeWrapper providerType)
            collection.TryAddSingleton(typeof(IComponentProvider), providerType.Value);

        if (builder.Properties.TryGetValue(nameof(IExecutionScope), out prop) && prop is TypeWrapper scopeType)
            collection.TryAddScoped(typeof(IExecutionScope), scopeType.Value);

        if (builder.Properties.TryGetValue(nameof(IDependencyResolver), out prop) && prop is TypeWrapper resolverType)
            collection.TryAddScoped(typeof(IDependencyResolver), resolverType.Value);

        // Register the result handlers if any are defined.
        if (builder.Properties.TryGetValue(nameof(IResultHandler), out prop) && prop is HashSet<TypeWrapper> handlers)
            foreach (var handler in handlers)
                collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IResultHandler), handler.Value));

        // This isn't customizable, as the logic is tightly coupled with the execution scope.
        collection.TryAddScoped(typeof(IContextAccessor<>), typeof(ContextAccessor<>));

        // Attempts to add the component provider to the service collection.
        collection.TryAddSingleton<CommandExecutionFactory>();
    }

    #endregion
}
