using Commands.Helpers;
using Commands.Resolvers;
using Commands.Results;
using Commands.TypeConverters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Commands.Core
{
    /// <summary>
    ///     Represents a builder that is responsible for configuring a <see cref="IServiceCollection"/> for use with a <see cref="CommandManager"/>.
    /// </summary>
    /// <remarks>
    ///     This builder is responsible for configuring the following:
    ///     <list type="bullet">
    ///         <item>Registration of <see cref="TypeConverterBase"/> implementations to convert custom types.</item>
    ///         <item>Registration of <see cref="ResultResolverBase"/> implementations to handle post-execution data.</item>
    ///         <item>Registration of <see cref="SourceResolverBase"/> implementations to handle pre-execution command generation.</item>
    ///         <item>Configuration of base post-execution handlers through delegates.</item>
    ///         <item>Configuration of base pre-execution handlers through delegates.</item>
    ///         <item>Configuration of the <see cref="BuildOptions"/> responsible for the build process of commands and modules.</item>
    ///     </list>
    ///     In post-build processing, the context provided by builder will also do the following:
    ///     <list type="bullet">
    ///         <item>Discovery of <see cref="ModuleBase"/> through the provided <see cref="BuildOptions.Assemblies"/> for scope creation.</item>
    ///         <item>Setup of the default <see cref="CommandFinalizer"/> to dispose command scopes and publish results.</item>
    ///         <item>Setup of the default <see cref="CommandGenerator"/> to activate source resolvers and run commands.</item>
    ///     </list>
    /// </remarks>
    /// <typeparam name="T">The implementation of the <see cref="CommandManager"/> to be configured.</typeparam>
    public class HostManagerBuilder<T>(IServiceCollection services) : ManagerBuilder<T>(services)
        where T : CommandManager
    {
        private bool actionset = false;

        /// <summary>
        ///     Configures an action that runs when a command publishes its result. This action runs after all pipeline actions have been resolved.
        /// </summary>
        /// <remarks>
        ///     The <see cref="IRunResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="IRunResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <param name="sourceGenerator">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual HostManagerBuilder<T> AddSourceResolver(Func<SourceResult> sourceGenerator)
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
        ///     The <see cref="IRunResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="IRunResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <param name="sourceGenerator">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual HostManagerBuilder<T> AddSourceResolver(Func<ValueTask<SourceResult>> sourceGenerator)
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
        ///     Adds a <see cref="SourceResolverBase"/> to the <see cref="ManagerBuilder{T}.Services"/> of this builder that will later be injected into the configured <see cref="CommandGenerator"/> for source creation.
        /// </summary>
        /// <typeparam name="TResolver">The implementation type of <see cref="ResultResolverBase"/> to add to the <see cref="ManagerBuilder{T}.Services"/>.</typeparam>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual HostManagerBuilder<T> AddSourceResolver<TResolver>()
            where TResolver : SourceResolverBase
        {
            var descriptor = ServiceDescriptor.Singleton<SourceResolverBase, TResolver>();

            Services.TryAddEnumerable(descriptor);

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="SourceResolverBase"/> to the <see cref="ManagerBuilder{T}.Services"/> of this builder that will later be injected into the configured <see cref="CommandGenerator"/> for source craetion.
        /// </summary>
        /// <typeparam name="TResolver">The implementation type of <see cref="ResultResolverBase"/> to add to the <see cref="ManagerBuilder{T}.Services"/>.</typeparam>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual HostManagerBuilder<T> AddSourceResolver<TResolver>(TResolver resolver)
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

        /// <inheritdoc cref="ManagerBuilder{T}.FinalizeConfiguration(object[])"/>
        public new HostManagerBuilder<T> FinalizeConfiguration(params object[] parameters)
        {
            if (!Configured)
            {
                AddStarter();
            }

            base.FinalizeConfiguration(parameters);

            return this;
        }

        /// <summary>
        ///     Adds the <see cref="CommandFinalizer"/> to the <see cref="ManagerBuilder{T}.Services"/> for emitting a looping pattern to the injected <see cref="SourceResolverBase"/>'s"/>.
        /// </summary>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        protected virtual HostManagerBuilder<T> AddStarter()
        {
            Services.AddHostedService<CommandGenerator>();

            return this;
        }
    }
}
