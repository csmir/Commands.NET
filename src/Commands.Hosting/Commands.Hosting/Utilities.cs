using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Commands.Hosting;

/// <summary>
///     A static class containing methods for configuring a .NET Generic host -being any implementation of <see cref="IHostBuilder"/>- with Commands.NET functionality.
/// </summary>
public static class Utilities
{
    /// <summary>
    ///     Configures the <see cref="IHostBuilder"/> to use the default <see cref="IComponentProvider"/> and <see cref="CommandExecutionFactory"/>. 
    ///     Calling this method multiple times will attempt to add new services if they are not already registered, otherwise ignoring them.
    /// </summary>
    /// <remarks>
    ///     This method configures the <see cref="IServiceProvider"/> consumed by the <see cref="IHost"/> built from this builder, to implement the following services:
    ///     <list type="bullet">
    ///         <item>A singleton implementation of <see cref="CommandExecutionFactory"/>. This factory manages command scopes and execution lifetime.</item>
    ///         <item>A default singleton implementation of <see cref="IComponentProvider"/>. This provider supplies the defined <see cref="CommandExecutionFactory"/> with executable commands.</item>
    ///         <item>A default scoped implementation of <see cref="IDependencyResolver"/> which manages the scope's service injection for modules and statically -or delegate- defined commands.</item>
    ///         <item>A default scoped implementation of <see cref="IExecutionScope"/> which holds execution metadata for the scope of the command lifetime, and can be injected freely within said scope.</item>
    ///         <item>A default scoped implementation of <see cref="IContextAccessor{TContext}"/>. This accessor exposes the context by accessing it from the defined <see cref="IExecutionScope"/>.</item>
    ///         <item>A collection of singleton <see cref="ResultHandler"/> implementations. These handlers will be executed to process results of pipeline invocation.</item>
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

            AddComponentProvider(services, properties);
        });

        return builder;
    }

    /// <summary>
    ///     Adds a <see cref="IComponentProvider"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    ///     This method will try to add any resources not yet added to the service collection.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceProvider"/> to add the configured services to.</param>
    /// <param name="configureAction">An action to configure the <see cref="ComponentBuilderContext"/> which will be used to populate all related services.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for call-chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configureAction"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddComponentProvider(this IServiceCollection services, Action<ComponentBuilderContext> configureAction)
    {
        Assert.NotNull(configureAction, nameof(configureAction));

        var builder = new ComponentBuilderContext();

        configureAction(builder);

        AddComponentProvider(services, builder);

        return services;
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

    #region Internals

    internal static IHostBuilder ConfigureComponents(this IHostBuilder builder, ComponentBuilderContext componentBuilder)
    {
        Assert.NotNull(componentBuilder, nameof(componentBuilder));

        builder.ConfigureServices((ctx, services) =>
        {
            AddComponentProvider(services, componentBuilder);
        });

        return builder;
    }

    private static void AddComponentProvider(IServiceCollection services, ComponentBuilderContext builder)
        => TryAddServices(services, builder);

    private static void TryAddServices(IServiceCollection collection, ComponentBuilderContext builder)
    {
        collection.TryAddSingleton(typeof(IComponentProvider), builder.GetTypeProperty(nameof(IComponentProvider), typeof(ComponentProvider)));
        collection.TryAddScoped(typeof(IDependencyResolver), builder.GetTypeProperty(nameof(IDependencyResolver), typeof(KeyedDependencyResolver)));
        collection.TryAddScoped(typeof(IExecutionScope), builder.GetTypeProperty(nameof(IExecutionScope), typeof(ExecutionScope)));

        collection.TryAddScoped(typeof(IContextAccessor<>), typeof(ContextAccessor<>));

        collection.TryAddSingleton<CommandExecutionFactory>();

        if (builder.TryGetProperty<HashSet<Type>>(nameof(ResultHandler), out var resultsProperty))
        {
            var descriptors = resultsProperty.Select(([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] type) =>
            {
                return ServiceDescriptor.Singleton(typeof(ResultHandler), type);
            });

            foreach (var descriptor in descriptors)
                collection.TryAddEnumerable(descriptor);
        }
    }

    #endregion
}
