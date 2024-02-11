using Commands.Helpers;
using Commands.Reflection;
using Commands.Resolvers;
using Commands.TypeConverters;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Commands
{
    /// <summary>
    ///     A set of options determining the build process for modules and commands.
    /// </summary>
    public class BuildOptions()
    {
        const string DEFAULT_REGEX = @"^[a-z0-9_-]*$";

        private Dictionary<Type, TypeConverterBase>? keyedConverters;

        private readonly List<Action<BuildOptions>> _commandAdders = [];

        /// <summary>
        ///     Gets or sets a collection of assemblies that are to be used to discover created modules.
        /// </summary>
        /// <remarks>
        ///     Default: <see cref="Assembly.GetEntryAssembly"/>
        /// </remarks>
        public List<Assembly> Assemblies { get; set; } = [Assembly.GetEntryAssembly()!]; // never null in managed context.

        /// <summary>
        ///     Gets or sets a collection of <see cref="TypeConverterBase"/>'s representing predefined <see cref="Type"/> conversion.
        /// </summary>
        /// <remarks>
        ///     This range can be manipulated to remove base converters that should be replaced by local implementations, or to add new ones.
        ///     <br/>
        ///     <br/>
        ///     Default: <see cref="TypeConverterBase.BuildDefaults"/>.
        /// </remarks>
        public List<TypeConverterBase> TypeConverters { get; set; } = [.. TypeConverterBase.BuildDefaults()];

        /// <summary>
        ///     Gets or sets a collection of <see cref="ResultResolverBase"/>'s that serve as post-execution handlers.
        /// </summary>
        public List<ResultResolverBase> ResultResolvers { get; set; } = [];

        /// <summary>
        ///     Gets or sets a collection of <see cref="CommandInfo"/>'s that are manually created before the registration process runs.
        /// </summary>
        public List<CommandInfo> Commands { get; set; } = [];

        /// <summary>
        ///     Gets or sets the naming convention of commands and groups being registered into the <see cref="CommandManager"/>.
        /// </summary>
        /// <remarks>
        ///     Default: <c>@"^[a-z0-9_-]*$"</c>
        /// </remarks>
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'. We don't do this because it can be changed in source.
        public Regex NamingRegex { get; set; } = new(DEFAULT_REGEX, RegexOptions.Compiled);
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

        internal Dictionary<Type, TypeConverterBase> KeyedConverters
        {
            get
            {
                return keyedConverters ??= TypeConverters.ToDictionary(x => x.Type, x => x);
            }
        }

        /// <summary>
        ///     Configures an action that runs when a command publishes its result. This action runs after all pipeline actions have been resolved.
        /// </summary>
        /// <remarks>
        ///     The <see cref="ICommandResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="ICommandResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="BuildOptions"/> for call-chaining.</returns>
        public virtual BuildOptions AddResultResolver(Action<ConsumerBase, ICommandResult, IServiceProvider> resultAction)
        {
            if (resultAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resultAction);
            }

            ResultResolvers.Add(new DelegateResolver(resultAction));

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
        /// <returns>The same <see cref="BuildOptions"/> for call-chaining.</returns>
        public virtual BuildOptions AddResultResolver(Func<ConsumerBase, ICommandResult, IServiceProvider, ValueTask> resultAction)
        {
            if (resultAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resultAction);
            }

            ResultResolvers.Add(new AsyncDelegateResolver(resultAction));

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="ResultResolverBase"/> to the <see cref="ResultResolvers"/>.
        /// </summary>
        /// <typeparam name="TResolver">The implementation type of <see cref="ResultResolverBase"/> to add.</typeparam>
        /// <param name="resolver">The implementation of <see cref="ResultResolverBase"/> to add.</param>
        /// <returns>The same <see cref="BuildOptions"/> for call-chaining.</returns>
        public virtual BuildOptions AddResultResolver<TResolver>(TResolver resolver)
            where TResolver : ResultResolverBase
        {
            if (resolver == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resolver);
            }

            ResultResolvers.Add(resolver);

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="TypeConverterBase"/> to the <see cref="TypeConverters"/>.
        /// </summary>
        /// <typeparam name="TConverter">The implementation type of <see cref="TypeConverterBase"/> to add.</typeparam>
        /// <param name="converter">The implementation of <see cref="TypeConverterBase"/> to add.</param>
        /// <returns>The same <see cref="BuildOptions"/> for call-chaining.</returns>
        public virtual BuildOptions AddTypeConverter<TConverter>(TConverter converter)
            where TConverter : TypeConverterBase
        {
            if (converter == null)
            {
                ThrowHelpers.ThrowInvalidArgument(converter);
            }

            TypeConverters.Add(converter);

            return this;
        }

        /// <summary>
        ///     Configures the <see cref="Assemblies"/> with an additional assembly.
        /// </summary>
        /// <param name="assembly">An assembly that should be added to <see cref="BuildOptions.Assemblies"/>.</param>
        /// <returns>The same <see cref="BuildOptions"/> for call-chaining.</returns>
        public virtual BuildOptions AddAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                ThrowHelpers.ThrowInvalidArgument(assembly);
            }

            Assemblies.Add(assembly);

            return this;
        }

        /// <summary>
        ///     Configures the <see cref="Assemblies"/> with an additional set of assemblies.
        /// </summary>
        /// <param name="assemblies">A collection of assemblies that should be added to <see cref="BuildOptions.Assemblies"/>.</param>
        /// <returns>The same <see cref="BuildOptions"/> for call-chaining.</returns>
        public virtual BuildOptions AddAssemblies(params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                ThrowHelpers.ThrowInvalidArgument(assemblies);
            }

            Assemblies.AddRange(assemblies);
            return this;
        }

        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the list of <see cref="Commands"/>.
        /// </summary>
        /// <remarks>
        ///     Delegate commands <b>require</b> the first parameter to be <see cref="CommandContext"/>, which holds scope and execution information of the created command during its execution.
        /// </remarks>
        /// <param name="name">The command name.</param>
        /// <param name="commandAction">The action of the command.</param>
        /// <param name="aliases">The aliases of the command.</param>
        /// <returns>The same <see cref="BuildOptions"/> for call-chaining.</returns>
        public virtual BuildOptions AddCommand(string name, Delegate commandAction, params string[] aliases)
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

        /// <summary>
        ///     
        /// </summary>
        public void FinalizeConfiguration()
        {
            foreach (var action in _commandAdders)
            {
                action(this);
            }
        }
    }
}
