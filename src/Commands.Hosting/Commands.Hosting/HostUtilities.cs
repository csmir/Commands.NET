using System.ComponentModel;

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
        => ConfigureComponents(builder, (_, ctx) => { });

    /// <inheritdoc cref="ConfigureComponents(IHostBuilder)"/>
    /// <param name="builder">The builder to configure with the related services.</param>
    /// <param name="configureComponents">An action to configure the <see cref="ComponentBuilderContext"/> which will be used to populate all related services.</param>
    /// <returns>The same <see cref="IHostBuilder"/> for call-chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureComponents"/> is <see langword="null"/>.</exception>
    public static IHostBuilder ConfigureComponents(this IHostBuilder builder, Action<ComponentBuilderContext> configureComponents)
    {
        Assert.NotNull(configureComponents, nameof(configureComponents));

        return ConfigureComponents(builder, (_, ctx) => configureComponents(ctx));
    }

    /// <inheritdoc cref="ConfigureComponents(IHostBuilder, Action{ComponentBuilderContext})"/>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureComponents"/> is <see langword="null"/>.</exception>
    public static IHostBuilder ConfigureComponents(this IHostBuilder builder, Action<HostBuilderContext, ComponentBuilderContext> configureComponents)
    {
        Assert.NotNull(configureComponents, nameof(configureComponents));

        var properties = new ComponentBuilderContext();

        builder.ConfigureServices((ctx, services) =>
        {
            configureComponents(ctx, properties);

            ServiceUtilities.AddComponentProvider(services, properties);
        });

        return builder;
    }

    /// <inheritdoc cref="ConfigureComponents(IHostBuilder, Action{ComponentBuilderContext})"/>
    /// <param name="builder">The builder to configure with the related services.</param>
    /// <param name="componentBuilder">The configuration to use for these components.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IHostBuilder ConfigureComponents(this IHostBuilder builder, ComponentBuilderContext componentBuilder)
    {
        Assert.NotNull(componentBuilder, nameof(componentBuilder));

        builder.ConfigureServices((ctx, services) =>
        {
            ServiceUtilities.AddComponentProvider(services, componentBuilder);
        });

        return builder;
    }

    /// <summary>
    ///     Configures the <see cref="IHost"/>'s <see cref="IComponentProvider"/> to use the provided settings of the <see cref="ComponentTree"/> as the source of components.
    /// </summary>
    /// <param name="host">The host to configure with the related components.</param>
    /// <param name="configureTree">An action to configure the <see cref="ComponentTree"/> with available components.</param>
    /// <returns>The same <see cref="IHost"/> for call chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureTree"/> is <see langword="null"/>.</exception>
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
