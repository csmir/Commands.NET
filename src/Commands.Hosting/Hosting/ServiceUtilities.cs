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
    ///     This method will remove any existing <see cref="IComponentProvider"/> and <see cref="ICommandExecutionFactory"/> from the collection before adding newly configured instances. Additionally, it configures a <see cref="IExecutionContext"/> and <see cref="ICallerContextAccessor{TCaller}"/> under scoped context.
    /// </remarks>
    /// <typeparam name="TFactory">The type implementing <see cref="CommandExecutionFactory"/> which will be used to create execution context and fire off commands with.</typeparam>
    /// <param name="services">The <see cref="IServiceProvider"/> to add the configured services to.</param>
    /// <param name="configureAction"></param>
    /// <returns></returns>
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
            services.RemoveAll<IExecutionContext>();
            services.RemoveAll<IDependencyResolver>();
            services.RemoveAll(typeof(ICallerContextAccessor<>));
        }

        services.AddSingleton<ICommandExecutionFactory, TFactory>();
        services.AddSingleton<IDependencyResolver, KeyedDependencyResolver>();

        services.AddScoped<IExecutionContext, ExecutionContext>();
        services.AddTransient(typeof(ICallerContextAccessor<>), typeof(CallerContextAccessor<>));

        var providerDescriptor = ServiceDescriptor.Singleton(x =>
        {
            // Implement global result handler to dispose of the execution scope. This must be done last, even if the properties are mutated anywhere before.
            properties.AddHandler(new ExecutionScopeResolver());

            return new ComponentProvider(properties.GetHandlers());
        });

        services.Add(providerDescriptor);

        return services;
    }
}
