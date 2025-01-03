using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Commands.Builders;

/// <summary>
///     A utility class that provides extension methods for configuring services with a hosted execution sequence targetting <see cref="IComponentTree"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class BuilderUtilities
{
    /// <summary>
    ///     Configures the <see cref="IHostBuilder"/> with a hosted execution sequence targetting <see cref="IComponentTree"/>. 
    ///     Repeated calls to this method will create new instances of the <see cref="IComponentTree"/>, with its own configuration.
    /// </summary>
    /// <param name="hostBuilder">The builder which implements all new instances of <see cref="IComponentTree"/>.</param>
    /// <param name="configureAction">An action that configures a newly created instance implementation of <see cref="ITreeBuilder"/>, which will be used to create a new <see cref="IComponentTree"/>.</param>
    /// <returns>The same <see cref="IHostBuilder"/> for call-chaining.</returns>
    public static IHostBuilder ConfigureCommands(this IHostBuilder hostBuilder, Action<ITreeBuilder> configureAction)
        => ConfigureCommands(hostBuilder, (_, builder) => configureAction(builder));

    /// <inheritdoc cref="ConfigureCommands(IHostBuilder, Action{ITreeBuilder})"/>
    public static IHostBuilder ConfigureCommands(this IHostBuilder hostBuilder, Action<HostBuilderContext, ITreeBuilder> configureAction)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            var treeBuilder = new ComponentTreeBuilder();

            configureAction(context, treeBuilder);

            services.Add(treeBuilder.ToDescriptor());
        });

        return hostBuilder;
    }

    /// <summary>
    ///     Creates a new <see cref="ServiceDescriptor"/> from the <see cref="ITreeBuilder"/> instance, which when added to a service collection will provide a singleton instance of <see cref="IComponentTree"/>.
    /// </summary>
    /// <remarks>
    ///     By adding this descriptor to an <see cref="IServiceCollection"/>, the collection ensures that the <see cref="IComponentTree"/> instance is created only once it is injected into another service, and not any earlier.
    ///     This method does not call <see cref="ITreeBuilder.Build"/>, letting this be done only by the service provider when the instance is requested.
    /// </remarks>
    /// <param name="builder">The builder which is used to create a new instance of <see cref="IComponentTree"/>.</param>
    /// <param name="serviceLifetime">The lifetime of the service. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <returns>A newly created <see cref="ServiceDescriptor"/> representing an <see cref="IComponentTree"/> created from the provided <see cref="ITreeBuilder"/>.</returns>
    public static ServiceDescriptor ToDescriptor(this ITreeBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        => new(typeof(IComponentTree), (provider) => builder.Build(), serviceLifetime);
}
