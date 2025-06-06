using Commands.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Commands.Http;

/// <summary>
///     A utility class containing methods for working with HTTP hosts in the Commands.NET framework.
/// </summary>
public static class HttpUtilities
{
    /// <inheritdoc cref="HostUtilities.ConfigureComponents(IHostBuilder, Action{ComponentBuilderContext})"/>
    public static IHostBuilder ConfigureHttpComponents(this IHostBuilder builder, Action<ComponentBuilderContext> configureComponents)
        => ConfigureHttpComponents(builder, (context, httpBuilder) => configureComponents(httpBuilder));

    /// <inheritdoc cref="HostUtilities.ConfigureComponents(IHostBuilder, Action{ComponentBuilderContext})"/>
    public static IHostBuilder ConfigureHttpComponents(this IHostBuilder builder, Action<HostBuilderContext, ComponentBuilderContext> configureComponents)
    {
        Assert.NotNull(configureComponents, nameof(configureComponents));

        var httpBuilder = new ComponentBuilderContext();

        builder.ConfigureServices((context, services) =>
        {
            configureComponents(context, httpBuilder);

            DefineServices(services, httpBuilder);
        });

        return builder.ConfigureComponents(httpBuilder);
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

        if (!builder.Properties.TryGetValue(nameof(HttpListener), out var listenerObj) || listenerObj is not HttpListener listener)
        {
            listener = new HttpListener();

            builder.Properties[nameof(HttpListener)] = listener;
        }

        configure(listener);

        return builder;
    }

    /// <summary>
    ///     Configures the default result handler to be used by the HTTP commands. If this method is not called, the default result handler will be <see cref="HttpResultHandler"/> which writes the result to the HTTP response.
    /// </summary>
    /// <typeparam name="THandler">The type of handler which will be the default for handling HTTP results.</typeparam>
    /// <param name="builder">The builder to configure.</param>
    /// <returns>The same <see cref="ComponentBuilderContext"/> for call chaining.</returns>
    public static ComponentBuilderContext WithDefaultResultHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicNestedTypes)] THandler>
        (this ComponentBuilderContext builder)
        where THandler : HttpResultHandler
    {
        builder.Properties[nameof(HttpResultHandler)] = new TypeWrapper(typeof(THandler));

        return builder;
    }

    #region Internals

    // A method that defines the services to be added to the service collection.
    internal static void DefineServices(IServiceCollection collection, ComponentBuilderContext builder)
    {
        Assert.NotNull(collection, nameof(collection));

        if (collection.Contains<HttpListener>())
        {
            // Remove the existing listener to avoid conflicts.
            collection.RemoveAll<HttpListener>();
        }

        if (builder.Properties.TryGetValue(nameof(HttpListener), out var prop) && prop is HttpListener listener)
            collection.AddSingleton(listener);
        else // Configuring the listener is mandatory.
            throw new InvalidOperationException($"No {nameof(HttpListener)} configured. Use {nameof(ConfigureListener)} to configure the listener with the required prefixes and additional properties.");

        if (builder.Properties.TryGetValue(nameof(HttpResultHandler), out prop) && prop is TypeWrapper handlerType)
            collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(ResultHandler), handlerType.Value));
        else
            collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(ResultHandler), typeof(HttpResultHandler)));
    }

    #endregion
}
