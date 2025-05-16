namespace Commands.Hosting;

/// <summary>
///     A static class containing methods for configuring a .NET Generic host -being any implementation of <see cref="IHostBuilder"/>- with Commands.NET functionality.
/// </summary>
public static class HostUtilities
{
    /// <summary>
    ///     Configures the <see cref="IHostBuilder"/> to use the default <see cref="IComponentProvider"/> and defined <see cref="ICommandExecutionFactory"/>.
    /// </summary>
    /// <remarks>
    ///     This method configures the <see cref="IServiceProvider"/> consumed by the <see cref="IHost"/> built from this builder, to implement the following services:
    ///     <list type="bullet">
    ///         <item>A singleton implementation of <see cref="ICommandExecutionFactory"/> as defined by the provided type, if any. This factory manages command scopes and execution lifetime.</item>
    ///         <item>A default singleton implementation of <see cref="IComponentProvider"/>. This provider supplies the defined <see cref="ICommandExecutionFactory"/> with executable commands.</item>
    ///         <item>A default scoped implementation of <see cref="IDependencyResolver"/> which manages the scope's service injection for modules and statically -or delegate- defined commands.</item>
    ///         <item>A default scoped implementation of <see cref="IExecutionScope"/> which holds execution metadata for the scope of the command lifetime, and can be injected freely within said scope.</item>
    ///         <item>A default scoped implementation of <see cref="IContextAccessor{TContext}"/>. This accessor exposes the context by accessing it from the defined <see cref="IExecutionScope"/>.</item>
    ///     </list>
    /// </remarks>
    /// <param name="builder">The builder to configure with the related services.</param>
    /// <returns>The same <see cref="IHostBuilder"/> for call-chaining.</returns>
    public static IHostBuilder ConfigureComponents(this IHostBuilder builder)
        => ConfigureComponents<CommandExecutionFactory>(builder, (_, ctx) => { });

    /// <inheritdoc cref="ConfigureComponents(IHostBuilder)"/>
    /// <param name="builder">The builder to configure with the related services.</param>
    /// <param name="configureComponents">An action to configure the <see cref="ComponentBuilder"/> which will be used to populate all related services.</param>
    /// <returns>The same <see cref="IHostBuilder"/> for call-chaining.</returns>
    public static IHostBuilder ConfigureComponents(this IHostBuilder builder, Action<ComponentBuilder> configureComponents)
    {
        Assert.NotNull(configureComponents, nameof(configureComponents));

        return ConfigureComponents<CommandExecutionFactory>(builder, (_, ctx) => configureComponents(ctx));
    }

    /// <inheritdoc cref="ConfigureComponents(IHostBuilder)"/>
    /// <typeparam name="TFactory">The type implementing <see cref="CommandExecutionFactory"/> which will be used to create execution context and fire off commands with.</typeparam>
    /// <param name="builder">The builder to configure with the related services.</param>
    /// <param name="configureComponents">An action to configure the <see cref="ComponentBuilder"/> which will be used to populate all related services.</param>
    /// <returns>The same <see cref="IHostBuilder"/> for call-chaining.</returns>
    public static IHostBuilder ConfigureComponents<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>(
        this IHostBuilder builder, Action<ComponentBuilder> configureComponents)
        where TFactory : CommandExecutionFactory
    {
        Assert.NotNull(configureComponents, nameof(configureComponents));

        return ConfigureComponents<TFactory>(builder, (_, ctx) => configureComponents(ctx));
    }

    /// <inheritdoc cref="ConfigureComponents{TFactory}(IHostBuilder, Action{ComponentBuilder})"/>
    public static IHostBuilder ConfigureComponents<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>(
        this IHostBuilder builder, Action<HostBuilderContext, ComponentBuilder> configureComponents)
        where TFactory : CommandExecutionFactory
    {
        Assert.NotNull(configureComponents, nameof(configureComponents));

        var properties = new ComponentBuilder();

        builder.ConfigureServices((ctx, services) =>
        {
            configureComponents(ctx, properties);
            ServiceUtilities.AddComponentProvider<TFactory>(services, properties);
        });

        return builder;
    }

    /// <summary>
    ///     Configures the <see cref="IHost"/>'s <see cref="IComponentProvider"/> to use the provided settings of the <see cref="ComponentTree"/> as the source of components.
    /// </summary>
    /// <param name="host">The host to configure with the related components.</param>
    /// <param name="configureTree">An action to configure the <see cref="ComponentTree"/> with available components.</param>
    /// <returns>The same <see cref="IHost"/> for call chaining.</returns>
    public static IHost UseComponents(this IHost host, Action<ComponentTree> configureTree)
    {
        Assert.NotNull(configureTree, nameof(configureTree));

        return UseComponents(host, (_, tree) => configureTree(tree));
    }

    /// <inheritdoc cref="UseComponents(IHost, Action{ComponentTree})"/>
    public static IHost UseComponents(this IHost host, Action<IServiceProvider, ComponentTree> configureTree)
    {
        Assert.NotNull(configureTree, nameof(configureTree));

        var provider = host.Services.GetRequiredService<IComponentProvider>();

        configureTree(host.Services, provider.Components);

        return host;
    }
}
