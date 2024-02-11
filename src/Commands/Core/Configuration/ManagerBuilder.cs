using Commands.Helpers;
using Commands.Reflection;
using Commands.Resolvers;
using Commands.TypeConverters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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
    ///         <item>Configuration of base post-execution handlers through delegates.</item>
    ///         <item>Configuration of the <see cref="BuildOptions"/> responsible for the build process of commands and modules.</item>
    ///     </list>
    ///     In post-build processing, the context provided by builder will also do the following:
    ///     <list type="bullet">
    ///         <item>Discovery of <see cref="ModuleBase"/> through the provided <see cref="BuildOptions.Assemblies"/> for scope creation.</item>
    ///         <item>Setup of the default <see cref="CommandFinalizer"/> to dispose command scopes and publish results.</item>
    ///     </list>
    /// </remarks>
    /// <typeparam name="T">The implementation of the <see cref="CommandManager"/> to be configured.</typeparam>
    public class ManagerBuilder<T>
        where T : CommandManager
    {
        private bool actionset = false;

        private readonly List<Action<BuildOptions>> _commandAdders = [];

        /// <summary>
        ///     Gets the services to be configured with added resolvers, converters and options.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        ///     Gets a set of options that configure the building process for discovered modules and commands by the <see cref="CommandManager"/>.
        /// </summary>
        /// <remarks>
        ///     This property can be manipulated by calling <see cref="AddOptions(Action{BuildOptions})"/>. 
        ///     The <see cref="AddAssemblies(Assembly[])"/> and <see cref="AddAssembly(Assembly)"/> methods are responsible for adding individual assemblies.
        /// </remarks>
        public BuildOptions Options { get; } = new();

        /// <summary>
        ///     Gets or sets whether this builder has called <see cref="FinalizeConfiguration(object[])"/> yet.
        /// </summary>
        protected bool Configured { get; set; } = false;

        /// <summary>
        ///     Creates a new <see cref="ManagerBuilder{T}"/> from the provided <paramref name="services"/>.
        /// </summary>
        /// <param name="services">The collection of services that should be used to configure this <see cref="ManagerBuilder{T}"/>.</param>
        public ManagerBuilder([DisallowNull] IServiceCollection services)
        {
            if (services == null)
            {
                ThrowHelpers.ThrowInvalidArgument(services);
            }

            Services = services;
        }

        #region Resolvers
        /// <summary>
        ///     Configures an action that runs when a command publishes its result. This action runs after all pipeline actions have been resolved.
        /// </summary>
        /// <remarks>
        ///     The <see cref="ICommandResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="ICommandResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual ManagerBuilder<T> AddResultResolver(Action<ConsumerBase, ICommandResult, IServiceProvider> resultAction)
        {
            if (actionset)
            {
                ThrowHelpers.ThrowInvalidOperation("A delegate result action has already been configured for this builder.");
            }

            if (resultAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resultAction);
            }

            var descriptor = ServiceDescriptor.Singleton(typeof(ResultResolverBase), new DelegateResolver(resultAction));

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
        /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual ManagerBuilder<T> AddResultResolver(Func<ConsumerBase, ICommandResult, IServiceProvider, ValueTask> resultAction)
        {
            if (actionset)
            {
                ThrowHelpers.ThrowInvalidOperation("A delegate result action has already been configured for this builder.");
            }

            if (resultAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resultAction);
            }

            var descriptor = ServiceDescriptor.Singleton(typeof(ResultResolverBase), new AsyncDelegateResolver(resultAction));

            Services.TryAddEnumerable(descriptor);

            actionset = true;

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="ResultResolverBase"/> to the <see cref="Services"/> of this builder that will later be injected into the configured <see cref="CommandFinalizer"/> for result handling.
        /// </summary>
        /// <typeparam name="TResolver">The implementation type of <see cref="ResultResolverBase"/> to add to the <see cref="Services"/>.</typeparam>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual ManagerBuilder<T> AddResultResolver<TResolver>()
            where TResolver : ResultResolverBase
        {
            var descriptor = ServiceDescriptor.Singleton<ResultResolverBase, TResolver>();

            Services.TryAddEnumerable(descriptor);

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="ResultResolverBase"/> to the <see cref="Services"/> of this builder that will later be injected into the configured <see cref="CommandFinalizer"/> for result handling.
        /// </summary>
        /// <typeparam name="TResolver">The implementation type of <see cref="ResultResolverBase"/> to add to the <see cref="Services"/>.</typeparam>
        /// <param name="resolver">The implementation of <see cref="ResultResolverBase"/> to add to the <see cref="Services"/>.</param>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual ManagerBuilder<T> AddResultResolver<TResolver>(TResolver resolver)
            where TResolver : ResultResolverBase
        {
            if (resolver == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resolver);
            }

            var descriptor = ServiceDescriptor.Singleton(resolver);

            Services.TryAddEnumerable(descriptor);

            return this;
        }
        #endregion

        #region Converters
        /// <summary>
        ///     Adds a <see cref="TypeConverterBase"/> to the <see cref="Services"/> of this builder that will later be injected into the <see cref="CommandManager"/> for command registration.
        /// </summary>
        /// <typeparam name="TConverter">The implementation type of <see cref="TypeConverterBase"/> to add to the <see cref="Services"/>.</typeparam>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual ManagerBuilder<T> AddTypeConverter<TConverter>()
            where TConverter : TypeConverterBase
        {
            var descriptor = ServiceDescriptor.Singleton<TypeConverterBase, TConverter>();

            Services.TryAddEnumerable(descriptor);

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="TypeConverterBase"/> to the <see cref="Services"/> of this builder that will later be injected into the <see cref="CommandManager"/> for command registration.
        /// </summary>
        /// <typeparam name="TConverter">The implementation type of <see cref="TypeConverterBase"/> to add to the <see cref="Services"/>.</typeparam>
        /// <param name="converter">The implementation of <see cref="TypeConverterBase"/> to add to the <see cref="Services"/>.</param>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual ManagerBuilder<T> AddTypeConverter<TConverter>(TConverter converter)
            where TConverter : TypeConverterBase
        {
            if (converter == null)
            {
                ThrowHelpers.ThrowInvalidArgument(converter);
            }
            var descriptor = ServiceDescriptor.Singleton(converter);

            Services.TryAddEnumerable(descriptor);

            return this;
        }
        #endregion

        #region Assemblies
        /// <summary>
        ///     Configures the <see cref="Options"/> with an additional assembly.
        /// </summary>
        /// <param name="assembly">An assembly that should be added to <see cref="BuildOptions.Assemblies"/>.</param>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual ManagerBuilder<T> AddAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                ThrowHelpers.ThrowInvalidArgument(assembly);
            }

            Options.Assemblies = [assembly, .. Options.Assemblies];

            return this;
        }

        /// <summary>
        ///     Configures the <see cref="Options"/> with an additional set of assemblies.
        /// </summary>
        /// <param name="assemblies">A collection of assemblies that should be added to <see cref="BuildOptions.Assemblies"/>.</param>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual ManagerBuilder<T> AddAssemblies(params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                ThrowHelpers.ThrowInvalidArgument(assemblies);
            }

            Options.Assemblies = [.. assemblies, .. Options.Assemblies];
            return this;
        }
        #endregion

        #region Commands
        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the <see cref="BuildOptions"/>.
        /// </summary>
        /// <remarks>
        ///     Delegate commands <b>require</b> the first parameter to be <see cref="CommandContext"/>, which holds scope and execution information of the created command during its execution.
        /// </remarks>
        /// <param name="name">The command name.</param>
        /// <param name="commandAction">The action of the command.</param>
        /// <param name="aliases">The aliases of the command.</param>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual ManagerBuilder<T> AddCommand(string name, Delegate commandAction, params string[] aliases)
        {
            if (commandAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(commandAction);
            }

            if (aliases == null)
            {
                ThrowHelpers.ThrowInvalidArgument(aliases);
            }

            aliases = [name, .. aliases];

            var action = new Action<BuildOptions>((BuildOptions options) =>
            {
                foreach (var alias in aliases)
                {
                    if (aliases.Contains(alias))
                    {
                        ThrowHelpers.ThrowNotDistinct(alias);
                    }

                    if (!options.NamingRegex.IsMatch(alias))
                    {
                        ThrowHelpers.ThrowNotMatched(alias);
                    }
                }

                options.Commands.Add(new CommandInfo(new DelegateInvoker(commandAction.Method, commandAction.Target), aliases, options));
            });

            _commandAdders.Add(action);

            return this;
        }
        #endregion

        /// <summary>
        ///     Configures the <see cref="Options"/> of this builder.
        /// </summary>
        /// <param name="configure">The action with which the options should be configured.</param>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual ManagerBuilder<T> AddOptions(Action<BuildOptions> configure)
        {
            if (configure == null)
            {
                ThrowHelpers.ThrowInvalidArgument(configure);
            }

            configure(Options);

            return this;
        }

        /// <summary>
        ///     Finalizes the configuration of this <see cref="ManagerBuilder{T}"/>, configuring module registration and the command finalizer.
        /// </summary>
        /// <remarks>
        ///     If the builder is configured via the generic HostBuilder or <see cref="ServiceCollection"/>, this call is unnecessary.
        /// </remarks>
        /// <param name="parameters">Constructor arguments of <typeparamref name="T"/> that are not provided by the <see cref="Services"/>.</param>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        public virtual ManagerBuilder<T> FinalizeConfiguration(params object[] parameters)
        {
            if (Configured)
            {
                return this;
            }

            AddFinalizer();

            AddModules();
            AddCommands();

            var descriptor = ServiceDescriptor.Singleton((services) =>
            {
                return ActivatorUtilities.CreateInstance<T>(services, [Options, .. parameters]);
            });

            Services.TryAdd(descriptor);

            Configured = true;

            return this;
        }

        /// <summary>
        ///     Adds all modules known by <see cref="BuildOptions.Assemblies"/> to the <see cref="Services"/> for registration.
        /// </summary>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        protected virtual void AddModules()
        {
            var rootType = typeof(ModuleBase);

            foreach (var assembly in Options.Assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (rootType.IsAssignableFrom(type) && !type.IsAbstract && !type.ContainsGenericParameters)
                    {
                        Services.TryAddTransient(type);
                    }
                }
            }
        }

        /// <summary>
        ///     Adds all command delegates as configured by <see cref="AddCommand(string, Delegate, string[])"/> to the <see cref="BuildOptions"/>.
        /// </summary>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        protected virtual void AddCommands()
        {
            foreach (var action in _commandAdders)
            {
                action(Options);
            }
        }

        /// <summary>
        ///     Adds the <see cref="CommandFinalizer"/> to the <see cref="Services"/> for result publishing and scope disposal.
        /// </summary>
        /// <returns>The same <see cref="ManagerBuilder{T}"/> for call-chaining.</returns>
        protected virtual void AddFinalizer()
        {
            Services.TryAddSingleton<CommandFinalizer>();
        }

        /// <summary>
        ///     Creates a new <see cref="ManagerBuilder{T}"/> to configure a <see cref="CommandManager"/> with options and implementations.
        /// </summary>
        /// <param name="services"></param>
        /// <returns>A new <see cref="ManagerBuilder{T}"/> from the provided <paramref name="services"/>.</returns>
        public static ManagerBuilder<T> CreateBuilder(IServiceCollection services)
        {
            return new ManagerBuilder<T>(services);
        }
    }
}
