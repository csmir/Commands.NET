using System.Diagnostics.CodeAnalysis;

namespace Commands.Hosting;

/// <summary>
///     
/// </summary>
public static class HostUtilities
{
    /// <summary>
    ///     
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureCommands(this IHostBuilder builder) 
        => ConfigureCommands(builder, configure => { });

    /// <summary>
    ///     
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureAction"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureCommands(this IHostBuilder builder, Action<ComponentCollectionProperties> configureAction) 
        => ConfigureCommands(builder, configureAction);

    /// <summary>
    ///     
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureAction"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureCommands(this IHostBuilder builder, Action<HostBuilderContext, ComponentCollectionProperties> configureAction)
    {
        Assert.NotNull(builder, nameof(builder));
        Assert.NotNull(configureAction, nameof(configureAction));

        var properties = new ComponentCollectionProperties();

        var services = builder.ConfigureServices((ctx, services) =>
        {
            configureAction(ctx, properties);

            var collectionSingleton = ServiceDescriptor.Singleton(x =>
            {
                // Implement global result handler to dispose of the collection. This must be done last.
                properties.AddResultHandler(new ScopeResultHandler());

                var collection = properties.Create();

                return collection;
            });
            var configurationSingleton = ServiceDescriptor.Singleton(x =>
            {
                var configuration = x.GetRequiredService<ComponentCollection>().Configuration;

                return configuration;
            });
            var executionContextScope = ServiceDescriptor.Scoped<IExecutionContext, ExecutionContext>();

            services.Add(collectionSingleton);
            services.Add(configurationSingleton);
            services.Add(executionContextScope);
        });

        return builder;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="TFactory"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostBuilder WithCommandFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>(this IHostBuilder builder)
        where TFactory : HostedCommandExecutionFactory
    {
        Assert.NotNull(builder, nameof(builder));

        var services = builder.ConfigureServices((ctx, services) =>
        {
            var factory = ServiceDescriptor.Singleton<ICommandExecutionFactory, TFactory>();
            services.Add(factory);
        });

        return builder;
    }
}
