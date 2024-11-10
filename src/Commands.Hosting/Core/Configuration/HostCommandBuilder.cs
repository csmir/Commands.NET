using Commands.Helpers;
using Commands.Resolvers;
using Commands.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Commands
{
    /// <summary>
    ///     Represents a builder that is responsible for configuring an 
    ///     <see cref="IServiceCollection"/> for use with a <see cref="CommandManager"/>.
    /// </summary>
    public static class HostCommandBuilder
    {
        /// <summary>
        ///     Configures the <see cref="IHostBuilder"/> to support use of a <see cref="CommandManager"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureDelegate">A delegate to configure the <see cref="CommandBuilder"/> with the current <see cref="IHostBuilder"/>'s building context.</param>
        /// <returns>The same <see cref="IHostBuilder"/> for call chaining.</returns>
        public static IHostBuilder ConfigureCommands(this IHostBuilder builder,
            Action<HostBuilderContext, HostCommandBuilder<CommandManager>> configureDelegate)
        {
            return builder.ConfigureCommands<CommandManager>(configureDelegate);
        }

        /// <summary>
        ///     Configures the <see cref="IHostBuilder"/> to support use of <typeparamref name="TManager"/> as an implementation of <see cref="CommandManager"/>.
        /// </summary>
        /// <typeparam name="TManager">The implementation of a <see cref="CommandManager"/> to configure for use.</typeparam>
        /// <param name="builder"></param>
        /// <param name="configureDelegate">A delegate to configure the <see cref="CommandBuilder"/> with the current <see cref="IHostBuilder"/>'s building context.</param>
        /// <returns>The same <see cref="IHostBuilder"/> for call chaining.</returns>
        public static IHostBuilder ConfigureCommands<TManager>(this IHostBuilder builder,
            Action<HostBuilderContext, HostCommandBuilder<TManager>> configureDelegate)
            where TManager : CommandManager
        {
            if (configureDelegate == null)
            {
                ThrowHelpers.ThrowInvalidArgument(configureDelegate);
            }

            builder.ConfigureServices((context, services) =>
            {
                var builder = new HostCommandBuilder<TManager>(services);

                configureDelegate(context, builder);

                builder.FinalizeConfiguration();
            });

            return builder;
        }

        /// <summary>
        ///     Configures the <see cref="IServiceCollection"/> for use of a <see cref="CommandManager"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns>The same <see cref="IServiceCollection"/> for call-chaining.</returns>
        public static IServiceCollection ConfigureCommands(this IServiceCollection collection)
        {
            return collection.ConfigureCommands<CommandManager>(x => { });
        }

        /// <summary>
        ///     Configures the <see cref="IServiceCollection"/> for use of a <see cref="CommandManager"/> with the provided builder configuration.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="configureDelegate">A delegate to configure the <see cref="HostCommandBuilder{T}"/> responsible for customizing the <see cref="CommandManager"/> setup.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for call-chaining.</returns>
        public static IServiceCollection ConfigureCommands(this IServiceCollection collection,
            Action<HostCommandBuilder<CommandManager>> configureDelegate)
        {
            collection.ConfigureCommands<CommandManager>(configureDelegate);

            return collection;
        }

        /// <summary>
        ///     Configures the <see cref="IServiceCollection"/> for use of a <see cref="CommandManager"/> with the provided builder configuration.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="configureDelegate">A delegate to configure the <see cref="CommandBuilder"/> responsible for customizing the <see cref="CommandManager"/> setup.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for call-chaining.</returns>
        public static IServiceCollection ConfigureCommands<T>(this IServiceCollection collection,
            Action<HostCommandBuilder<T>> configureDelegate)
            where T : CommandManager
        {
            if (configureDelegate == null)
            {
                ThrowHelpers.ThrowInvalidArgument(configureDelegate);
            }

            var options = new HostCommandBuilder<T>(collection);

            configureDelegate(options);

            var descriptor = ServiceDescriptor.Singleton((services) =>
            {
                return ActivatorUtilities.CreateInstance<T>(services, [options]);
            });

            collection.Add(descriptor);

            return collection;
        }
    }

    /// <summary>
    ///     Represents a builder that is responsible for configuring an 
    ///     <see cref="IServiceCollection"/> for use with a <see cref="CommandManager"/>.
    /// </summary>
    /// <typeparam name="T">The implementation of the <see cref="CommandManager"/> to be configured.</typeparam>
    public class HostCommandBuilder<T>(IServiceCollection services) : CommandBuilder<T>
        where T : CommandManager
    {
        private bool s_delegate_defined = false;

        /// <summary>
        ///     Gets the service collection that is to be configured for use with a <see cref="CommandManager"/>.
        /// </summary>
        public IServiceCollection Services { get; } = services;

        /// <summary>
        ///     Configures an action that runs when a command publishes its result. This action runs after all pipeline actions have been resolved.
        /// </summary>
        /// <remarks>
        ///     The <see cref="ICommandResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="ICommandResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <param name="sourceGenerator">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="HostCommandBuilder{T}"/> for call-chaining.</returns>
        public HostCommandBuilder<T> AddSourceResolver(Func<SourceResult> sourceGenerator)
        {
            if (s_delegate_defined)
            {
                ThrowHelpers.ThrowInvalidOperation("A delegate source generator has already been configured for this builder.");
            }

            if (sourceGenerator == null)
            {
                ThrowHelpers.ThrowInvalidArgument(sourceGenerator);
            }

            var descriptor = ServiceDescriptor.Singleton(typeof(SourceResolverBase), new DelegateResolver(sourceGenerator));

            Services.TryAddEnumerable(descriptor);

            s_delegate_defined = true;

            return this;
        }

        /// <summary>
        ///     Configures an asynchronous action that runs when a command publishes its result. This action runs after all pipeline actions have been resolved.
        /// </summary>
        /// <remarks>
        ///     The <see cref="ICommandResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="ICommandResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <param name="sourceGenerator">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="HostCommandBuilder{T}"/> for call-chaining.</returns>
        public HostCommandBuilder<T> AddSourceResolver(Func<ValueTask<SourceResult>> sourceGenerator)
        {
            if (s_delegate_defined)
            {
                ThrowHelpers.ThrowInvalidOperation("A delegate source generator has already been configured for this builder.");
            }

            if (sourceGenerator == null)
            {
                ThrowHelpers.ThrowInvalidArgument(sourceGenerator);
            }

            var descriptor = ServiceDescriptor.Singleton(typeof(SourceResolverBase), new AsyncDelegateResolver(sourceGenerator));

            Services.TryAddEnumerable(descriptor);

            s_delegate_defined = true;

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="SourceResolverBase"/> to the <see cref="HostCommandBuilder{T}.Services"/> of this builder that will later be injected into the configured <see cref="CommandGenerator"/> for source creation.
        /// </summary>
        /// <typeparam name="TResolver">The implementation type of <see cref="ResultResolverBase"/> to add to the <see cref="HostCommandBuilder{T}.Services"/>.</typeparam>
        /// <returns>The same <see cref="HostCommandBuilder{T}"/> for call-chaining.</returns>
        public HostCommandBuilder<T> AddSourceResolver<TResolver>()
            where TResolver : SourceResolverBase
        {
            var descriptor = ServiceDescriptor.Singleton<SourceResolverBase, TResolver>();

            Services.TryAddEnumerable(descriptor);

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="SourceResolverBase"/> to the <see cref="HostCommandBuilder{T}.Services"/> of this builder that will later be injected into the configured <see cref="CommandGenerator"/> for source craetion.
        /// </summary>
        /// <typeparam name="TResolver">The implementation type of <see cref="ResultResolverBase"/> to add to the <see cref="HostCommandBuilder{T}.Services"/>.</typeparam>
        /// <returns>The same <see cref="HostCommandBuilder{T}"/> for call-chaining.</returns>
        public HostCommandBuilder<T> AddSourceResolver<TResolver>(TResolver resolver)
            where TResolver : SourceResolverBase
        {
            if (resolver == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resolver);
            }

            var descriptor = ServiceDescriptor.Singleton<SourceResolverBase>(resolver);

            Services.TryAddEnumerable(descriptor);

            return this;
        }

        /// <summary>
        ///     Finalizes the configuration of this <see cref="HostCommandBuilder{T}"/>, configuring the manager and the source resolver.
        /// </summary>
        /// <remarks>
        ///     If the builder is configured via the generic HostBuilder or <see cref="ServiceCollection"/>, this call is unnecessary.
        /// </remarks>
        /// <returns>The same <see cref="HostCommandBuilder{T}"/> for call-chaining.</returns>
        public new HostCommandBuilder<T> FinalizeConfiguration()
        {
            AddStarter();

            var descriptor = ServiceDescriptor.Singleton((services) =>
            {
                return ActivatorUtilities.CreateInstance<T>(services, [this]);
            });

            Services.TryAdd(descriptor);

            base.FinalizeConfiguration();

            return this;
        }

        /// <summary>
        ///     Adds the <see cref="CommandFinalizer"/> to the <see cref="HostCommandBuilder{T}.Services"/> for emitting a looping pattern to the injected <see cref="SourceResolverBase"/>'s"/>.
        /// </summary>
        /// <returns>The same <see cref="HostCommandBuilder{T}"/> for call-chaining.</returns>
        protected HostCommandBuilder<T> AddStarter()
        {
            Services.AddHostedService<CommandGenerator>();

            return this;
        }
    }
}
