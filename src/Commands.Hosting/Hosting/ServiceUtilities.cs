using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Runtime.CompilerServices;

namespace Commands.Hosting;

/// <summary>
///     A static class containing methods for configuring a <see cref="IServiceProvider"/> with Commands.NET functionality.
/// </summary>
public static class ServiceUtilities
{
    /// <summary>
    ///     Configures the <see cref="IServiceCollection"/> with the default implementation of <see cref="IExecutableComponentSet"/>, the mechanism for executing commands. This method will replace all existing related services.
    /// </summary>
    /// <remarks>
    ///     This method adds a singleton implementation of <see cref="IExecutableComponentSet"/> and the <see cref="ComponentConfiguration"/> used to create it. 
    ///     Additionally, it provides a factory based execution mechanism for commands, implementing a singleton <see cref="ICommandExecutionFactory"/>, scoped <see cref="IExecutionContext"/> and transient <see cref="ICallerContextAccessor{TCaller}"/>.
    /// </remarks>
    /// <param name="services"></param>
    /// <param name="configureAction">An action responsible for configuring a newly created instance of <see cref="ComponentSetBuilder"/> in preparation for building an implementation of <see cref="IExecutableComponentSet"/> to execute commands with.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for call-chaining.</returns>
    public static IServiceCollection AddComponentCollection<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>
        (this IServiceCollection services, Action<ComponentSetBuilder> configureAction)
        where TFactory : class, ICommandExecutionFactory
    {
        Assert.NotNull(services, nameof(services));
        Assert.NotNull(configureAction, nameof(configureAction));

        var properties = new ComponentSetBuilder();

        configureAction(properties);

        return AddComponentCollection<TFactory>(services, properties);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IServiceCollection AddComponentCollection<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>
        (IServiceCollection services, ComponentSetBuilder properties)
        where TFactory : class, ICommandExecutionFactory
    {
        if (services.Contains<ICommandExecutionFactory>())
        {
            // Remove the existing factory to avoid conflicts.
            services.RemoveAll<ICommandExecutionFactory>();
            services.RemoveAll<IExecutableComponentSet>();
            services.RemoveAll<ComponentConfiguration>();
            services.RemoveAll<IExecutionContext>();
            services.RemoveAll(typeof(ICallerContextAccessor<>));
        }

        services.AddSingleton<ICommandExecutionFactory, TFactory>();

        services.AddScoped<IExecutionContext, ExecutionContext>();
        services.AddTransient(typeof(ICallerContextAccessor<>), typeof(CallerContextAccessor<>));

        var collectionDescriptor = ServiceDescriptor.Singleton<IExecutableComponentSet, ExecutableComponentSet>(x =>
        {
            // Implement global result handler to dispose of the execution scope. This must be done last, even if the properties are mutated anywhere before.
            properties.AddResultHandler(new ExecutionScopeResolver());

            var collection = properties.Build();

            return collection;
        });

        var configurationDescriptor = ServiceDescriptor.Singleton<ComponentConfiguration>(x =>
        {
            var provider = x.GetRequiredService<IExecutableComponentSet>();

            if (provider is ExecutableComponentSet collection)
                return collection.Configuration;

            throw new NotSupportedException($"The component collection is not available in the current context. Ensure that you are configuring an instance of {nameof(ExecutableComponentSet)}.");
        });

        services.Add(collectionDescriptor);
        services.Add(configurationDescriptor);

        return services;
    }
}
