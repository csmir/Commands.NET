using Commands.Resolvers;
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
        ///     which are control services that manage the execution of commands sourced from a collection of configured <see cref="SourceResolver"/> implementations.
        ///     A hosted service, being <see cref="InitiatorHostService"/>, will be added to the host to manage the lifecycle -bound to <see cref="IHostApplicationLifetime"/>- of the <see cref="ISequenceInitiator"/> collection which contains configured <see cref="IComponentTree"/> implementations. 
        ///     This collection's size is equal to the number of times this method was called before the host is started.
        /// </remarks>
        /// <param name="hostBuilder">The builder which implements all new instances of <see cref="IComponentTree"/>.</param>
        /// <param name="configureAction">An action that configures a newly created instance implementation of <see cref="ITreeBuilder"/>, which will be used to create a new <see cref="IComponentTree"/> for an <see cref="ISequenceInitiator"/>.</param>
        /// <returns>The same <see cref="IHostBuilder"/> for call-chaining.</returns>
        public static IHostBuilder ConfigureCommands(this IHostBuilder hostBuilder, Action<HostBuilderContext, ITreeBuilder> configureAction)
        {
            hostBuilder.ConfigureServices((context, services) =>
            {
                var treeBuilder = new ComponentTreeBuilder();

                configureAction(context, treeBuilder);

                var initiatorDescriptor = ServiceDescriptor.Singleton<ISequenceInitiator, SequenceInitiator>((provider) =>
                {
                    var tree = treeBuilder.Build();

                    if (!(treeBuilder.Configuration.Properties.TryGetValue("Commands:SourceResolvers", out var srcResolvers) && srcResolvers is List<SourceResolver> resolvers))
                        resolvers = [];

                    var logger = provider.GetRequiredService<ILogger<SequenceInitiator>>();

                    return new SequenceInitiator(tree, resolvers, provider, logger);
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
        ///     Adds a source resolver to the <see cref="ITreeBuilder"/> configuration. This resolver will be used to source data for the command execution.
        /// </summary>
        /// <remarks>
        ///     This action will be wrapped in a new instance of <see cref="DelegateSourceResolver"/>, and adds it to the <see cref="ITreeBuilder"/> configuration.
        /// </remarks>
        /// <param name="builder">The builder which is used to create new instances of <see cref="IComponentTree"/>.</param>
        /// <param name="sourcingAction">The action which should return a <see cref="SourceResult"/> representing a failed or succeeded retrieved command query.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public static ITreeBuilder AddSourceResolver(this ITreeBuilder builder, Func<IServiceProvider, SourceResult> sourcingAction)
        {
            if (!(builder.Configuration.Properties.TryGetValue("Commands:SourceResolvers", out var srcResolvers) && srcResolvers is List<SourceResolver> resolvers))
                resolvers = [];

            resolvers.Add(new DelegateSourceResolver(sourcingAction));

            builder.Configuration.Properties["Commands:SourceResolvers"] = resolvers;

            return builder;
        }

        /// <summary>
        ///     Adds a source resolver to the <see cref="ITreeBuilder"/> configuration. This resolver will be used to source data for the command execution.
        /// </summary>
        /// <remarks>
        ///     This action will be wrapped in a new instance of <see cref="AsyncDelegateSourceResolver"/>, and adds it to the <see cref="ITreeBuilder"/> configuration.
        /// </remarks>
        /// <param name="builder">The builder which is used to create new instances of <see cref="IComponentTree"/>.</param>
        /// <param name="sourcingAction">The action which should return a <see cref="SourceResult"/> representing a failed or succeeded retrieved command query.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public static ITreeBuilder AddSourceResolver(this ITreeBuilder builder, Func<IServiceProvider, ValueTask<SourceResult>> sourcingAction)
        {
            if (!(builder.Configuration.Properties.TryGetValue("Commands:SourceResolvers", out var srcResolvers) && srcResolvers is List<SourceResolver> resolvers))
                resolvers = [];

            resolvers.Add(new AsyncDelegateSourceResolver(sourcingAction));

            builder.Configuration.Properties["Commands:SourceResolvers"] = resolvers;

            return builder;
        }

        /// <summary>
        ///     Adds a source resolver to the <see cref="ITreeBuilder"/> configuration. This resolver will be used to source data for the command execution.
        /// </summary>
        /// <param name="builder">The builder which is used to create new instances of <see cref="IComponentTree"/>.</param>
        /// <param name="resolver">The resolver which should return a <see cref="SourceResult"/> representing a failed or succeeded retrieved command query.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public static ITreeBuilder AddSourceResolver(this ITreeBuilder builder, SourceResolver resolver)
        {
            if (!(builder.Configuration.Properties.TryGetValue("Commands:SourceResolvers", out var srcResolvers) && srcResolvers is List<SourceResolver> resolvers))
                resolvers = [];

            resolvers.Add(resolver);

            builder.Configuration.Properties["Commands:SourceResolvers"] = resolvers;

            return builder;
        }
    }
}
