namespace Commands.Hosting;

/// <summary>
///     A static class containing methods for configuring a .NET Generic host -being any implementation of <see cref="IHostBuilder"/>- with Commands.NET functionality.
/// </summary>
public static class HostUtilities
{
    ///// <summary>
    /////     Configures the <see cref="IHostBuilder"/> with the default implementation of <see cref="IComponentProvider"/>, the mechanism for executing commands. This method will replace all existing related services.
    ///// </summary>
    ///// <remarks>
    /////     This method adds a singleton implementation of <see cref="IComponentProvider"/> and the <see cref="ComponentConfiguration"/> used to create it. 
    /////     Additionally, it provides a factory based execution mechanism for commands, implementing a singleton <see cref="ICommandExecutionFactory"/>, scoped <see cref="IExecutionContext"/> and transient <see cref="ICallerContextAccessor{TCaller}"/>.
    ///// </remarks>
    ///// <param name="builder"></param>
    ///// <returns>The same <see cref="IHostBuilder"/> for call-chaining.</returns>
    //public static IHostBuilder ConfigureComponents(this IHostBuilder builder)
    //    => ConfigureComponents(builder, configure => { });

    ///// <inheritdoc cref="ConfigureComponents(IHostBuilder)"/>
    ///// <param name="builder"></param>
    ///// <param name="configureAction">An action responsible for configuring a newly created instance of <see cref="ComponentProviderBuilder"/> in preparation for building an implementation of <see cref="IComponentProvider"/> to execute commands with.</param>
    ///// <returns>The same <see cref="IHostBuilder"/> for call-chaining.</returns>
    //public static IHostBuilder ConfigureComponents(this IHostBuilder builder, Action<ComponentProviderContext> configureAction)
    //    => ConfigureComponents(builder, (ctx, props) => configureAction(props));

    ///// <inheritdoc cref="ConfigureComponents(IHostBuilder)"/>
    ///// <param name="builder"></param>
    ///// <param name="configureAction">An action responsible for configuring a newly created instance of <see cref="ComponentProviderBuilder"/> in preparation for building an implementation of <see cref="IComponentProvider"/> to execute commands with.</param>
    ///// <returns>The same <see cref="IHostBuilder"/> for call-chaining.</returns>
    //public static IHostBuilder ConfigureComponents(this IHostBuilder builder, Action<HostBuilderContext, ComponentProviderContext> configureAction)
    //    => ConfigureComponents<CommandExecutionFactory>(builder, configureAction);

    ///// <inheritdoc cref="ConfigureComponents(IHostBuilder)"/>
    ///// <typeparam name="TFactory">The implementation of <see cref="ICommandExecutionFactory"/> to consider the factory for executing commands using this host as the lifetime.</typeparam>
    ///// <param name="builder"></param>
    ///// <param name="configureAction">An action responsible for configuring a newly created instance of <see cref="ComponentProviderBuilder"/> in preparation for building an implementation of <see cref="IComponentProvider"/> to execute commands with.</param>
    ///// <returns>The same <see cref="IHostBuilder"/> for call-chaining.</returns>
    //public static IHostBuilder ConfigureComponents<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>
    //    (this IHostBuilder builder, Action<HostBuilderContext, ComponentProviderContext> configureAction)
    //    where TFactory : CommandExecutionFactory
    //{
    //    Assert.NotNull(builder, nameof(builder));
    //    Assert.NotNull(configureAction, nameof(configureAction));

    //    var properties = new ComponentProviderContext();
    //    var services = builder.ConfigureServices((ctx, services) =>
    //    {
    //        configureAction(ctx, properties);

    //        ServiceUtilities.AddComponentCollection<TFactory>(services, properties);
    //    });
    //    return builder;
    //}
}
