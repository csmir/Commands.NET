using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Runtime.CompilerServices;

namespace Commands.Hosting;

/// <summary>
///     A static class containing methods for configuring a <see cref="IServiceProvider"/> with Commands.NET functionality.
/// </summary>
public static class ServiceUtilities
{
    /// <summary>
    ///     Configures the <see cref="IServiceCollection"/> with the default implementation of <see cref="IExecutionProvider"/>, the mechanism for executing commands. This method will replace all existing related services.
    /// </summary>
    /// <remarks>
    ///     This method adds a singleton implementation of <see cref="IExecutionProvider"/> and the <see cref="ComponentConfiguration"/> used to create it. 
    ///     Additionally, it provides a factory based execution mechanism for commands, implementing a singleton <see cref="IExecutionFactory"/>, scoped <see cref="IExecutionContext"/> and transient <see cref="ICallerContextAccessor{TCaller}"/>.
    /// </remarks>
    /// <param name="services"></param>
    /// <param name="configureAction">An action responsible for configuring a newly created instance of <see cref="ComponentCollectionProperties"/> in preparation for building an implementation of <see cref="IExecutionProvider"/> to execute commands with.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for call-chaining.</returns>
    public static IServiceCollection AddComponentCollection<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>
        (this IServiceCollection services, Action<ComponentCollectionProperties> configureAction)
        where TFactory : class, IExecutionFactory
    {
        Assert.NotNull(services, nameof(services));
        Assert.NotNull(configureAction, nameof(configureAction));

        var properties = new ComponentCollectionProperties();

        configureAction(properties);

        return AddComponentCollection<TFactory>(services, properties);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IServiceCollection AddComponentCollection<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>
        (IServiceCollection services, ComponentCollectionProperties properties)
        where TFactory : class, IExecutionFactory
    {
        services.RemoveAll(typeof(IExecutionProvider));
        services.RemoveAll(typeof(ComponentConfiguration));
        services.RemoveAll(typeof(IExecutionFactory));

        services.AddSingleton<IExecutionFactory, TFactory>();

        services.AddScoped<IExecutionContext, ExecutionContext>();
        services.AddTransient(typeof(ICallerContextAccessor<>), typeof(CallerContextAccessor<>));

        var collectionDescriptor = ServiceDescriptor.Singleton<IExecutionProvider, ComponentCollection>(x =>
        {
            // Implement global result handler to dispose of the execution scope. This must be done last, even if the properties are mutated anywhere before.
            properties.AddResultHandler(new ExecutionScopeResolver());

            var collection = properties.Create();

            return collection;
        });

        var configurationDescriptor = ServiceDescriptor.Singleton<ComponentConfiguration>(x =>
        {
            var provider = x.GetRequiredService<IExecutionProvider>();

            if (provider is ComponentCollection collection)
                return collection.Configuration;

            throw new NotSupportedException($"The component collection is not available in the current context. Ensure that you are configuring an instance of {nameof(ComponentCollection)}.");
        });

        services.Add(collectionDescriptor);
        services.Add(configurationDescriptor);

        return services;
    }
}
