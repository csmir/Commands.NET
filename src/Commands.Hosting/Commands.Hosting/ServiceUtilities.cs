using Commands.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Runtime.CompilerServices;

namespace Commands.Hosting;

/// <summary>
///     A static class containing methods for configuring a <see cref="IServiceProvider"/> with Commands.NET functionality.
/// </summary>
public static class ServiceUtilities
{
    /// <summary>
    ///     Adds a <see cref="IComponentProvider"/> to the <see cref="IServiceCollection"/> using the provided <typeparamref name="TFactory"/> as the factory for creating execution contexts.
    /// </summary>
    /// <remarks>
    ///     This method will remove any existing <see cref="IComponentProvider"/> and <see cref="ICommandExecutionFactory"/> from the collection before adding newly configured instances. Additionally, it configures a <see cref="IExecutionScope"/> and <see cref="IContextAccessor{TContext}"/> under scoped context.
    /// </remarks>
    /// <typeparam name="TFactory">The type implementing <see cref="CommandExecutionFactory"/> which will be used to create execution context and fire off commands with.</typeparam>
    /// <param name="services">The <see cref="IServiceProvider"/> to add the configured services to.</param>
    /// <param name="configureAction"></param>
    /// <returns>The same <see cref="IServiceCollection"/> for call-chaining.</returns>
    public static IServiceCollection AddComponentProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>
        (this IServiceCollection services, Action<ComponentBuilder> configureAction)
        where TFactory : class, ICommandExecutionFactory
    {
        Assert.NotNull(services, nameof(services));
        Assert.NotNull(configureAction, nameof(configureAction));

        var properties = new ComponentBuilder();

        configureAction(properties);

        return AddComponentProvider<TFactory>(services, properties);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IServiceCollection AddComponentProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>
        (IServiceCollection services, ComponentBuilder properties)
        where TFactory : class, ICommandExecutionFactory
    {
        if (services.Contains<ICommandExecutionFactory>())
        {
            // Remove the existing factory to avoid conflicts.
            services.RemoveAll<ICommandExecutionFactory>();
            services.RemoveAll<IComponentProvider>();
            services.RemoveAll<IExecutionScope>();
            services.RemoveAll<IDependencyResolver>();
            services.RemoveAll(typeof(IContextAccessor<>));
        }

        services.AddSingleton<ICommandExecutionFactory, TFactory>();

        services.AddScoped<IDependencyResolver, KeyedDependencyResolver>();
        services.AddScoped<IExecutionScope, ExecutionContext>();
        services.AddScoped(typeof(IContextAccessor<>), typeof(ContextAccessor<>));

        var providerDescriptor = ServiceDescriptor.Singleton<IComponentProvider, ComponentProvider>(x =>
        {
            // Implement global result handler to dispose of the execution scope. This must be done last, even if the properties are mutated anywhere before.
            properties.AddResultHandler(new ExecutionScopeResolver());

            return new ComponentProvider(properties.ResultHandlers.ToArray());
        });

        services.Add(providerDescriptor);

        return services;
    }
}
