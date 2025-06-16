using Commands.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Commands.Http;

/// <summary>
///     A utility class containing methods for working with HTTP hosts in the Commands.NET framework.
/// </summary>
public static class Utilities
{
    /// <summary>
    ///     Configures the <see cref="IHostBuilder"/> to use an <see cref="IComponentProvider"/> and <see cref="HttpCommandExecutionFactory"/> for HTTP command execution.
    ///     Calling this method multiple times will attempt to add new services if they are not already registered, otherwise ignoring them.
    /// </summary>
    /// <remarks>
    ///     This method configures the <see cref="IServiceProvider"/> consumed by the <see cref="IHost"/> built from this builder, to implement the following services:
    ///     <list type="bullet">
    ///         <item>A required hosted singleton of <see cref="HttpCommandExecutionFactory"/>. This factory manages command scopes and execution lifetime.</item>
    ///         <item>A required singleton of <see cref="HttpListener"/>. This listener can be configured using <see cref="ConfigureListener(ComponentBuilderContext, Action{HttpListener})"/>.</item>
    ///         <item>A singleton implementation of <see cref="HttpResultHandler"/>. This handler is exclusively to handle unhandled HTTP results, and can be replaced or ignored by self-registered handlers of higher priority.</item>
    ///         <item>A singleton implementation of <see cref="IComponentProvider"/>. This provider supplies the defined <see cref="CommandExecutionFactory"/> with executable commands.</item>
    ///         <item>A scoped implementation of <see cref="IDependencyResolver"/> which manages the scope's service injection for modules and statically -or delegate- defined commands.</item>
    ///         <item>A scoped implementation of <see cref="IExecutionScope"/> which holds execution metadata for the scope of the command lifetime, and can be injected freely within said scope.</item>
    ///         <item>A scoped implementation of <see cref="IContextAccessor{TContext}"/>. This accessor exposes the context by accessing it from the defined <see cref="IExecutionScope"/>.</item>
    ///         <item>A collection of singleton <see cref="ResultHandler"/> implementations. These handlers can process command results irrespective of the response logic handled by the <see cref="HttpResultHandler"/>.</item>
    ///     </list>
    /// </remarks>
    /// <param name="builder">The builder to configure with the related services.</param>
    /// <param name="configureComponents">An action to configure the <see cref="ComponentBuilderContext"/> which will be used to populate all related services.</param>
    /// <returns>The same <see cref="IHostBuilder"/> for call-chaining.</returns>
    public static IHostBuilder ConfigureHttpComponents(this IHostBuilder builder, Action<ComponentBuilderContext> configureComponents)
        => ConfigureHttpComponents(builder, (context, httpBuilder) => configureComponents(httpBuilder));

    /// <inheritdoc cref="ConfigureHttpComponents(IHostBuilder, Action{ComponentBuilderContext})"/>
    public static IHostBuilder ConfigureHttpComponents(this IHostBuilder builder, Action<HostBuilderContext, ComponentBuilderContext> configureComponents)
    {
        Assert.NotNull(configureComponents, nameof(configureComponents));

        var httpBuilder = new ComponentBuilderContext();

        builder.ConfigureServices((context, services) =>
        {
            configureComponents(context, httpBuilder);
        });

        // Do the lower level configuration of the core components first.
        builder.ConfigureComponents(httpBuilder);

        builder.ConfigureServices((context, services) =>
        {
            TryAddServices(services, httpBuilder);
        });

        return builder;
    }

    /// <summary>
    ///     Configures an <see cref="HttpListener"/> instance to be used by the Commands.NET host with the provided action.
    /// </summary>
    /// <param name="builder">The builder to configure.</param>
    /// <param name="configure">An action to configure the listener object with.</param>
    /// <returns>The same <see cref="ComponentBuilderContext"/> for call chaining.</returns>
    public static ComponentBuilderContext ConfigureListener(this ComponentBuilderContext builder, Action<HttpListener> configure)
    {
        Assert.NotNull(configure, nameof(configure));

        if (!builder.TryGetProperty<HttpListener>(nameof(HttpListener), out var listenerProperty))
        {
            listenerProperty = new HttpListener();

            builder.Properties[nameof(HttpListener)] = listenerProperty;
        }

        configure(listenerProperty);

        return builder;
    }

    /// <summary>
    ///     Configures the default result handler to be used by the HTTP commands. If this method is not called, the default result handler will be <see cref="HttpResultHandler"/>, which writes the result to the HTTP response.
    /// </summary>
    /// <typeparam name="THandler">The type of handler which will be the default for handling HTTP results.</typeparam>
    /// <param name="builder">The builder to configure.</param>
    /// <returns>The same <see cref="ComponentBuilderContext"/> for call chaining.</returns>
    public static ComponentBuilderContext WithHttpHandler<THandler>
        (this ComponentBuilderContext builder)
        where THandler : HttpResultHandler
    {
        builder.Properties[nameof(HttpResultHandler)] = typeof(THandler);

        return builder;
    }

    /// <summary>
    ///     Configures the HTTP command execution factory to be used by the Commands.NET host. If this method is not called, the default factory will be <see cref="HttpCommandExecutionFactory"/>.
    /// </summary>
    /// <typeparam name="TFactory">The type of factory which will be the default for handling HTTP calls to the API and forwarding them to the component provider.</typeparam>
    /// <param name="builder">The builder to configure.</param>
    /// <returns>The same <see cref="ComponentBuilderContext"/> for call chaining.</returns>
    public static ComponentBuilderContext WithHttpFactory<TFactory>
        (this ComponentBuilderContext builder)
        where TFactory : HttpCommandExecutionFactory
    {
        builder.Properties[nameof(HttpCommandExecutionFactory)] = typeof(TFactory);
        return builder;
    }

    #region Internals

    // A method that defines the services to be added to the service collection.
    internal static void TryAddServices(IServiceCollection collection, ComponentBuilderContext builder)
    {
        if (!builder.TryGetProperty<HttpListener>(nameof(HttpListener), out var listenerProperty))
            throw new InvalidOperationException($"No {nameof(HttpListener)} configured. Use {nameof(ConfigureListener)} to configure the listener with the required prefixes and additional properties.");

        collection.TryAddSingleton(listenerProperty);

        collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(ResultHandler), builder.GetTypeProperty(nameof(HttpResultHandler), typeof(HttpResultHandler))));
        collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHostedService), builder.GetTypeProperty(nameof(HttpCommandExecutionFactory), typeof(HttpCommandExecutionFactory))));
    }

    #endregion
}
