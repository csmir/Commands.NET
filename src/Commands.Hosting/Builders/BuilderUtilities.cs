using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Commands.Builders
{
    /// <summary>
    ///     A utility class that provides extension methods for configuring services with a hosted execution sequence targetting <see cref="IComponentTree"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class BuilderUtilities
    {
        /// <summary>
        ///     Configures the <see cref="IHostBuilder"/> with a hosted execution sequence targetting <see cref="IComponentTree"/>. 
        ///     Repeated calls to this method will create new instances of the <see cref="IComponentTree"/>, with its own configuration and configured source resolvers.
        /// </summary>
        /// <remarks>
        ///     This operation will not inject new instances of <see cref="IComponentTree"/> into the host's <see cref="IServiceProvider"/>.
        ///     Instead, it will immediately set new instances in the parameters of isolated <see cref="ISequenceInitiator"/>, 
        ///     which are control services that manage the execution of commands sourced from a collection of configured <see cref="SourceProvider"/> implementations.
        ///     A hosted service, being <see cref="InitiatorHostService"/>, will be added to the host to manage the lifecycle -bound to <see cref="IHostApplicationLifetime"/>- of the <see cref="ISequenceInitiator"/> collection which contains configured <see cref="IComponentTree"/> implementations. 
        ///     This collection's size is equal to the number of times this method was called before the host is started.
        /// </remarks>
        /// <param name="hostBuilder">The builder which implements all new instances of <see cref="IComponentTree"/>.</param>
        /// <param name="configureAction">An action that configures a newly created instance implementation of <see cref="ITreeBuilder"/>, which will be used to create a new <see cref="IComponentTree"/> for an <see cref="ISequenceInitiator"/>.</param>
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

                var initiatorDescriptor = ServiceDescriptor.Singleton<ISequenceInitiator, SequenceInitiator>((provider) =>
                {
                    var tree = treeBuilder.Build();

                    if (!(treeBuilder.Configuration.Properties.TryGetValue("SourceProviders", out var srcProviders) && srcProviders is List<SourceProvider> providers))
                        providers = [];

                    var logger = provider.GetRequiredService<ILogger<SequenceInitiator>>();

                    return new SequenceInitiator(tree, providers, provider, logger);
                });

                if (!context.Properties.ContainsKey("Commands:InitiatorConfigured"))
                {
                    services.AddHostedService<InitiatorHostService>();

                    context.Properties["Commands:InitiatorConfigured"] = true;
                }
            });

            return hostBuilder;
        }

        /// <summary>
        ///     Adds a source provider to the <see cref="ITreeBuilder"/> configuration. This provider will be used to source data for the command execution.
        /// </summary>
        /// <remarks>
        ///     This action will be wrapped in a new instance of <see cref="DelegateSourceProvider"/>, and adds it to the <see cref="ITreeBuilder"/> configuration.
        /// </remarks>
        /// <param name="builder">The builder which is used to create new instances of <see cref="IComponentTree"/>.</param>
        /// <param name="sourcingAction">The action which should return a <see cref="SourceResult"/> representing a failed or succeeded retrieved command query.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public static ITreeBuilder AddSourceProvider(this ITreeBuilder builder, Func<IServiceProvider, SourceResult> sourcingAction)
        {
            if (!(builder.Configuration.Properties.TryGetValue("SourceProviders", out var srcProviders) && srcProviders is List<SourceProvider> providers))
                providers = [];

            providers.Add(new DelegateSourceProvider(sourcingAction));

            builder.Configuration.Properties["SourceProviders"] = providers;

            return builder;
        }

        /// <summary>
        ///     Adds a source provider to the <see cref="ITreeBuilder"/> configuration. This provider will be used to source data for the command execution.
        /// </summary>
        /// <remarks>
        ///     This action will be wrapped in a new instance of <see cref="AsyncDelegateSourceProvider"/>, and adds it to the <see cref="ITreeBuilder"/> configuration.
        /// </remarks>
        /// <param name="builder">The builder which is used to create new instances of <see cref="IComponentTree"/>.</param>
        /// <param name="sourcingAction">The action which should return a <see cref="SourceResult"/> representing a failed or succeeded retrieved command query.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public static ITreeBuilder AddSourceProvider(this ITreeBuilder builder, Func<IServiceProvider, ValueTask<SourceResult>> sourcingAction)
        {
            if (!(builder.Configuration.Properties.TryGetValue("SourceProviders", out var srcProviders) && srcProviders is List<SourceProvider> providers))
                providers = [];

            providers.Add(new AsyncDelegateSourceProvider(sourcingAction));

            builder.Configuration.Properties["SourceProviders"] = providers;

            return builder;
        }

        /// <summary>
        ///     Adds a source provider to the <see cref="ITreeBuilder"/> configuration. This provider will be used to source data for the command execution.
        /// </summary>
        /// <param name="builder">The builder which is used to create new instances of <see cref="IComponentTree"/>.</param>
        /// <param name="provider">The provider which should return a <see cref="SourceResult"/> representing a failed or succeeded retrieved command query.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public static ITreeBuilder AddSourceProvider(this ITreeBuilder builder, SourceProvider provider)
        {
            if (!(builder.Configuration.Properties.TryGetValue("SourceProviders", out var srcProviders) && srcProviders is List<SourceProvider> providers))
                providers = [];

            providers.Add(provider);

            builder.Configuration.Properties["SourceProviders"] = providers;

            return builder;
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
}
