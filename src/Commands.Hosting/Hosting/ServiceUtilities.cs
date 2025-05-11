using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Runtime.CompilerServices;

namespace Commands.Hosting;

/// <summary>
///     A static class containing methods for configuring a <see cref="IServiceProvider"/> with Commands.NET functionality.
/// </summary>
public static class ServiceUtilities
{
    ///// <summary>
    /////     Configures the <see cref="IServiceCollection"/> with the default implementation of <see cref="IComponentProvider"/>, the mechanism for executing commands. This method will replace all existing related services.
    ///// </summary>
    ///// <remarks>
    /////     This method adds a singleton implementation of <see cref="IComponentProvider"/>. 
    /////     Additionally, it provides a factory based execution mechanism for commands, implementing a singleton <see cref="ICommandExecutionFactory"/>, scoped <see cref="IExecutionContext"/> and transient <see cref="ICallerContextAccessor{TCaller}"/>.
    ///// </remarks>
    ///// <param name="services"></param>
    ///// <param name="configureAction">An action responsible for configuring a newly created instance of <see cref="ComponentProviderBuilder"/> in preparation for building an implementation of <see cref="IComponentProvider"/> to execute commands with.</param>
    ///// <returns>The same <see cref="IServiceCollection"/> for call-chaining.</returns>
    //public static IServiceCollection AddComponentCollection<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>
    //    (this IServiceCollection services, Action<ComponentProviderContext> configureAction)
    //    where TFactory : class, ICommandExecutionFactory
    //{
    //    Assert.NotNull(services, nameof(services));
    //    Assert.NotNull(configureAction, nameof(configureAction));

    //    var properties = new ComponentProviderContext();

    //    configureAction(properties);

    //    return AddComponentCollection<TFactory>(services, properties);
    //}

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //internal static IServiceCollection AddComponentCollection<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>
    //    (IServiceCollection services, ComponentProviderContext properties)
    //    where TFactory : class, ICommandExecutionFactory
    //{
    //    if (services.Contains<ICommandExecutionFactory>())
    //    {
    //        // Remove the existing factory to avoid conflicts.
    //        services.RemoveAll<ICommandExecutionFactory>();
    //        services.RemoveAll<IComponentProvider>();
    //        services.RemoveAll<IExecutionContext>();
    //        services.RemoveAll(typeof(ICallerContextAccessor<>));
    //    }

    //    services.AddSingleton<ICommandExecutionFactory, TFactory>();

    //    services.AddScoped<IExecutionContext, ExecutionContext>();
    //    services.AddTransient(typeof(ICallerContextAccessor<>), typeof(CallerContextAccessor<>));

    //    var providerDescriptor = ServiceDescriptor.Singleton(x =>
    //    {
    //        // Implement global result handler to dispose of the execution scope. This must be done last, even if the properties are mutated anywhere before.
    //        properties.Components.AddResultHandler(new ExecutionScopeResolver());

    //        var provider = properties.Components.Build(properties.Configuration);

    //        return provider;
    //    });

    //    services.Add(providerDescriptor);

    //    return services;
    //}
}
