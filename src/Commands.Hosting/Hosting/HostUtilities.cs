namespace Commands.Hosting;

/// <summary>
///     A static class containing methods for configuring a .NET Generic host -being any implementation of <see cref="IHostBuilder"/>- with Commands.NET functionality.
/// </summary>
public static class HostUtilities
{
    /// <summary>
    ///     Configures the <see cref="IHostBuilder"/> to use the default <see cref="IComponentProvider"/> and <see cref="ICommandExecutionFactory"/>.
    /// </summary>
    /// <param name="builder">The builder to configure with the related services.</param>
    /// <param name="configureComponents">An action to configure the <see cref="ComponentProviderBuilder"/> which will be used to populate all related services.</param>
    /// <returns>The same <see cref="IHostBuilder"/> for call-chaining.</returns>
    public static IHostBuilder ConfigureComponents(this IHostBuilder builder, Action<ComponentProviderBuilder> configureComponents)
    {
        Assert.NotNull(configureComponents, nameof(configureComponents));

        return ConfigureComponents<CommandExecutionFactory>(builder, (_, ctx) => configureComponents(ctx));
    }

    /// <summary>
    ///     Configures the <see cref="IHostBuilder"/> to use the default <see cref="IComponentProvider"/> and provided <typeparamref name="TFactory"/>.
    /// </summary>
    /// <typeparam name="TFactory">The type implementing <see cref="CommandExecutionFactory"/> which will be used to create execution context and fire off commands with.</typeparam>
    /// <param name="builder">The builder to configure with the related services.</param>
    /// <param name="configureComponents">An action to configure the <see cref="ComponentProviderBuilder"/> which will be used to populate all related services.</param>
    /// <returns>The same <see cref="IHostBuilder"/> for call-chaining.</returns>
    public static IHostBuilder ConfigureComponents<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>(
        this IHostBuilder builder, Action<ComponentProviderBuilder> configureComponents)
        where TFactory : CommandExecutionFactory
    {
        Assert.NotNull(configureComponents, nameof(configureComponents));

        return ConfigureComponents<TFactory>(builder, (_, ctx) => configureComponents(ctx));
    }

    /// <inheritdoc cref="ConfigureComponents{TFactory}(IHostBuilder, Action{ComponentProviderBuilder})"/>
    public static IHostBuilder ConfigureComponents<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>(
        this IHostBuilder builder, Action<HostBuilderContext, ComponentProviderBuilder> configureComponents)
        where TFactory : CommandExecutionFactory
    {
        Assert.NotNull(configureComponents, nameof(configureComponents));

        var properties = new ComponentProviderBuilder();

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
