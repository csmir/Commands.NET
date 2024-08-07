﻿using Commands.Helpers;
using Commands.Resolvers;
using Commands.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Commands
{
    /// <summary>
    ///     Represents a builder that is responsible for configuring an 
    ///     <see cref="IServiceCollection"/> for use with a <see cref="CommandManager"/>.
    /// </summary>
    /// <typeparam name="T">The implementation of the <see cref="CommandManager"/> to be configured.</typeparam>
    public class HostCommandBuilder<T>(IServiceCollection services) : CommandBuilder<T>
        where T : CommandManager
    {
        private bool actionset = false;

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
        public virtual HostCommandBuilder<T> AddSourceResolver(Func<SourceResult> sourceGenerator)
        {
            if (actionset)
            {
                ThrowHelpers.ThrowInvalidOperation("A delegate source generator has already been configured for this builder.");
            }

            if (sourceGenerator == null)
            {
                ThrowHelpers.ThrowInvalidArgument(sourceGenerator);
            }

            var descriptor = ServiceDescriptor.Singleton(typeof(SourceResolverBase), new DelegateResolver(sourceGenerator));

            Services.TryAddEnumerable(descriptor);

            actionset = true;

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
        public virtual HostCommandBuilder<T> AddSourceResolver(Func<ValueTask<SourceResult>> sourceGenerator)
        {
            if (actionset)
            {
                ThrowHelpers.ThrowInvalidOperation("A delegate source generator has already been configured for this builder.");
            }

            if (sourceGenerator == null)
            {
                ThrowHelpers.ThrowInvalidArgument(sourceGenerator);
            }

            var descriptor = ServiceDescriptor.Singleton(typeof(SourceResolverBase), new AsyncDelegateResolver(sourceGenerator));

            Services.TryAddEnumerable(descriptor);

            actionset = true;

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="SourceResolverBase"/> to the <see cref="HostCommandBuilder{T}.Services"/> of this builder that will later be injected into the configured <see cref="CommandGenerator"/> for source creation.
        /// </summary>
        /// <typeparam name="TResolver">The implementation type of <see cref="ResultResolverBase"/> to add to the <see cref="HostCommandBuilder{T}.Services"/>.</typeparam>
        /// <returns>The same <see cref="HostCommandBuilder{T}"/> for call-chaining.</returns>
        public virtual HostCommandBuilder<T> AddSourceResolver<TResolver>()
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
        public virtual HostCommandBuilder<T> AddSourceResolver<TResolver>(TResolver resolver)
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
        protected virtual HostCommandBuilder<T> AddStarter()
        {
            Services.AddHostedService<CommandGenerator>();

            return this;
        }
    }
}
