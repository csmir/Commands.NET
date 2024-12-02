using Commands.Converters;
using Commands.Reflection;
using Commands.Resolvers;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Commands
{
    /// <summary>
    ///     A container for options determining the build process for modules and commands.
    /// </summary>
    public class ConfigurationBuilder
    {
        const string DEFAULT_REGEX = @"^[a-z0-9_-]*$";

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
        ///     Default: <see cref="TypeConverterBase.BuildDefaults"/>
        /// </remarks>
        public Dictionary<Type, TypeConverterBase> TypeConverters { get; set; } = TypeConverterBase.BuildDefaults();

        /// <summary>
        ///     Gets or sets a collection of <see cref="ResultResolverBase"/>'s that serve as post-execution handlers.
        /// </summary>
        public List<ResultResolverBase> ResultResolvers { get; set; } = [];

        /// <summary>
        ///     Gets or sets a collection of <see cref="CommandInfo"/>'s that are manually created before the registration process runs.
        /// </summary>
        public List<IComponentBuilder> Components { get; set; } = [];

        /// <summary>
        ///     Gets or sets the naming convention of commands and groups being registered into the <see cref="CommandManager"/>.
        /// </summary>
        /// <remarks>
        ///     Default: <c>@"^[a-z0-9_-]*$"</c>
        /// </remarks>
        public Regex NamingRegex { get; set; } = new(DEFAULT_REGEX, RegexOptions.Compiled);

        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the command collection.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="commandAction">The action of the command. Delegate commands are adviced to set the first parameter to be <see cref="CommandContext{T}"/>, which holds scope and execution information of the created command during its execution.</param>
        /// <param name="aliases">The aliases of the command.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ConfigurationBuilder AddCommand(string name, Delegate commandAction, params string[] aliases)
        {
            if (commandAction == null)
                throw new ArgumentNullException(nameof(commandAction));

            if (aliases == null)
                throw new ArgumentNullException(nameof(aliases));

            aliases = new string[] { name }
                .Concat(aliases)
                .Distinct()
                .ToArray();

            Components.Add(new CommandBuilder(aliases, commandAction));

            return this;
        }

        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the command collection.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="commandAction">The action of the command. Delegate commands are adviced to set the first parameter to be <see cref="CommandContext{T}"/>, which holds scope and execution information of the created command during its execution.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ConfigurationBuilder AddCommand(string name, Delegate commandAction)
        {
            return AddCommand(name, commandAction, []);
        }

        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the command collection.
        /// </summary>
        /// <param name="command">The command. Delegate commands are adviced to set the first parameter to be <see cref="CommandContext{T}"/>, which holds scope and execution information of the created command during its execution.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ConfigurationBuilder AddCommand(CommandBuilder command)
        {
            Components.Add(command);

            return this;
        }

        /// <summary>
        ///     Configures an action that runs when a command publishes its result. This action runs after all pipeline actions have been resolved.
        /// </summary>
        /// <remarks>
        ///     The <see cref="ICommandResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="ICommandResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ConfigurationBuilder AddResultResolver(Action<ConsumerBase, ICommandResult, IServiceProvider> resultAction)
        {
            if (resultAction == null)
                throw new ArgumentNullException(nameof(resultAction));

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
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ConfigurationBuilder AddResultResolver(Func<ConsumerBase, ICommandResult, IServiceProvider, ValueTask> resultAction)
        {
            if (resultAction == null)
                throw new ArgumentNullException(nameof(resultAction));

            ResultResolvers.Add(new AsyncDelegateResolver(resultAction));

            return this;
        }

        /// <summary>
        ///     Adds an implementation of <see cref="ResultResolverBase"/> to <see cref="ResultResolvers"/>.
        /// </summary>
        /// <typeparam name="TResolver">The implementation type of <see cref="ResultResolverBase"/> to add.</typeparam>
        /// <param name="resolver">The implementation of <see cref="ResultResolverBase"/> to add.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ConfigurationBuilder AddResultResolver<TResolver>(TResolver resolver)
            where TResolver : ResultResolverBase
        {
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            ResultResolvers.Add(resolver);

            return this;
        }

        /// <summary>
        ///     Configures an action that will convert a raw argument into the target type, signified by <typeparamref name="TConvertable"/>.
        /// </summary>
        /// <typeparam name="TConvertable">The type for this converter to target.</typeparam>
        /// <param name="convertAction">The action that is responsible for the conversion process.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ConfigurationBuilder AddTypeConverter<TConvertable>(Func<ConsumerBase, IArgument, object?, IServiceProvider, ConvertResult> convertAction)
        {
            if (convertAction == null)
                throw new ArgumentNullException(nameof(convertAction));

            var converter = new DelegateConverter<TConvertable>(convertAction);

            TypeConverters[converter.Type] = converter;

            return this;
        }

        /// <summary>
        ///     Configures an asynchronous action that will convert a raw argument into the target type, signified by <typeparamref name="TConvertable"/>.
        /// </summary>
        /// <typeparam name="TConvertable">The type for this converter to target.</typeparam>
        /// <param name="convertAction">The action that is responsible for the conversion process.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ConfigurationBuilder AddTypeConverter<TConvertable>(Func<ConsumerBase, IArgument, object?, IServiceProvider, ValueTask<ConvertResult>> convertAction)
        {
            if (convertAction == null)
                throw new ArgumentNullException(nameof(convertAction));

            var converter = new AsyncDelegateConverter<TConvertable>(convertAction);

            TypeConverters[converter.Type] = converter;

            return this;
        }

        /// <summary>
        ///     Adds an implementation of <see cref="TypeConverterBase"/> to <see cref="TypeConverters"/>.
        /// </summary>
        /// <typeparam name="TConverter">The implementation type of <see cref="TypeConverterBase"/> to add.</typeparam>
        /// <param name="converter">The implementation of <see cref="TypeConverterBase"/> to add.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ConfigurationBuilder AddTypeConverter<TConverter>(TConverter converter)
            where TConverter : TypeConverterBase
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            TypeConverters[converter.Type] = converter;

            return this;
        }

        /// <summary>
        ///     Configures the <see cref="Assemblies"/> with an additional assembly.
        /// </summary>
        /// <param name="assembly">An assembly that should be added to <see cref="Assemblies"/>.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ConfigurationBuilder AddAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            Assemblies.Add(assembly);

            return this;
        }

        /// <summary>
        ///     Configures the <see cref="Assemblies"/> with an additional set of assemblies.
        /// </summary>
        /// <param name="assemblies">A collection of assemblies that should be added to <see cref="Assemblies"/>.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ConfigurationBuilder AddAssemblies(params Assembly[] assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            Assemblies.AddRange(assemblies);
            return this;
        }

        /// <summary>
        ///     Builds the current <see cref="ConfigurationBuilder"/> instance into a new <see cref="CommandConfiguration"/>, which is used to create a new instance of <see cref="CommandManager"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="CommandManager"/> built by this builder.</returns>
        public CommandManager Build()
        {
            var configuration = new CommandConfiguration(this);

            return new CommandManager(configuration);
        }
    }
}
