using Commands.Converters;
using Commands.Reflection;
using Commands.Resolvers;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Commands
{
    /// <summary>
    ///     A configuration model determining the build process for modules and commands in a <see cref="CommandTree"/>. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     This builder is able to configure the following:
    ///     <list type="number">
    ///         <item>Defining assemblies through which will be searched to discover modules and commands.</item>
    ///         <item>Defining custom commands that do not appear in the assemblies.</item>
    ///         <item>Registering implementations of <see cref="TypeConverterBase"/> which define custom argument conversion.</item>
    ///         <item>Registering implementations of <see cref="ResultResolverBase"/> which define custom result handling.</item>
    ///         <item>Custom naming patterns that validate naming across the whole process.</item>
    ///     </list>
    /// </remarks>
    public sealed class CommandTreeBuilder
    {
        const string DEFAULT_REGEX = @"^[a-z0-9_-]*$";

        /// <summary>
        ///     Gets or sets a collection of assemblies that are to be used to discover created modules.
        /// </summary>
        public List<Assembly> Assemblies { get; set; }

        /// <summary>
        ///     Gets or sets a collection of <see cref="TypeConverterBase"/>'s representing predefined <see cref="Type"/> conversion.
        /// </summary>
        public Dictionary<Type, TypeConverterBase> TypeConverters { get; set; }

        /// <summary>
        ///     Gets or sets a collection of <see cref="ResultResolverBase"/>'s that serve as post-execution handlers.
        /// </summary>
        public List<ResultResolverBase> ResultResolvers { get; set; }

        /// <summary>
        ///     Gets or sets a collection of <see cref="CommandInfo"/>'s that are manually created before the registration process runs.
        /// </summary>
        public List<IComponentBuilder> Components { get; set; }

        /// <summary>
        ///     Gets or sets the naming convention of commands and groups being registered into the <see cref="CommandTree"/>.
        /// </summary>
        public Regex? NamingRegex { get; set; }

        /// <summary>
        ///     Gets or sets if registered modules should be made read-only at creation, meaning they cannot be modified at runtime.
        /// </summary>
        public bool SealModuleDefinitions { get; set; }

        /// <summary>
        ///     Gets or sets a filter that determines whether a created component should be yielded back to the registration process or skipped entirely, based on state provided within the component itself.
        /// </summary>
        public Func<IComponent, bool> RegisterComponentFilter { get; set; }

        /// <summary>
        ///     Creates a new instance of <see cref="CommandTreeBuilder"/>, with default values applied.
        /// </summary>
        public CommandTreeBuilder()
            : this(true)
        {

        }

        internal CommandTreeBuilder(bool applyStandards)
        {
            if (applyStandards)
            {
                NamingRegex = new(DEFAULT_REGEX, RegexOptions.Compiled);

                Assemblies      = [Assembly.GetEntryAssembly()!];
                TypeConverters  = TypeConverterBase.BuildDefaults();
            }
            else
            {
                Assemblies      = [];
                TypeConverters  = [];
            }

            ResultResolvers = [];
            Components      = [];

            SealModuleDefinitions = false;
            RegisterComponentFilter = _ => true;
        }

        /// <summary>
        ///     Adds a command to the <see cref="Components"/> collection.
        /// </summary>
        /// <param name="commandBuilder">The builder instance to add to the collection, which will be built into a <see cref="CommandInfo"/> instance that can be executed by the <see cref="CommandTree"/>.</param>
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided builder is <see langword="null"/>.</exception>
        public CommandTreeBuilder AddCommand(CommandBuilder commandBuilder)
        {
            if (commandBuilder == null)
                throw new ArgumentNullException(nameof(commandBuilder));

            Components.Add(commandBuilder);

            return this;
        }

        /// <summary>
        ///     Adds a command to the <see cref="Components"/> collection.
        /// </summary>
        /// <remarks>
        ///     When using this method, the command will be created with the default constructor. In order for the command to be valid for execution, <see cref="ModuleBuilder.WithAliases(string[])"/> must be called within <paramref name="configureCommand"/>.
        /// </remarks>
        /// <param name="configureCommand">An action that extends the fluent API of this type to configure the command.</param>
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        public CommandTreeBuilder AddCommand(Action<CommandBuilder> configureCommand)
        {
            var commandBuilder = new CommandBuilder();

            configureCommand(commandBuilder);

            return AddCommand(commandBuilder);
        }

        /// <summary>
        ///     Adds a command to the <see cref="Components"/> collection.
        /// </summary>
        /// <remarks>
        ///     Delegate based commands are able to access the command's scope by implementing <see cref="CommandContext{T}"/> as the first argument in the lambda signature.
        /// </remarks>
        /// <param name="name">The name of the component.</param>
        /// <param name="executionDelegate">The delegate to execute when the provided name of this object is discovered in a search operation.</param>
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        public CommandTreeBuilder AddCommand(string name, Delegate executionDelegate)
        {
            var commandBuilder = new CommandBuilder(name, [], executionDelegate);

            return AddCommand(commandBuilder);
        }

        /// <summary>
        ///     Adds a command to the <see cref="Components"/> collection.
        /// </summary>
        /// <remarks>
        ///     Delegate based commands are able to access the command's scope by implementing <see cref="CommandContext{T}"/> as the first argument in the lambda signature.
        /// </remarks>
        /// <param name="name">The name of the component.</param>
        /// <param name="executionDelegate">The delegate to execute when the provided name of this object is discovered in a search operation.</param>
        /// <param name="aliases">The aliases of the component, excluding the name.</param>
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        public CommandTreeBuilder AddCommand(string name, Delegate executionDelegate, params string[] aliases)
        {
            var commandBuilder = new CommandBuilder(name, aliases, executionDelegate);

            return AddCommand(commandBuilder);
        }

        /// <summary>
        ///     Adds a module to the <see cref="Components"/> collection.
        /// </summary>
        /// <param name="moduleBuilder">The builder instance to add to the collection, which will be built into a <see cref="ModuleInfo"/> instance that can contain commands to be executed by the <see cref="CommandTree"/>.</param>
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided builder is <see langword="null"/>.</exception>
        public CommandTreeBuilder AddModule(ModuleBuilder moduleBuilder)
        {
            if (moduleBuilder == null)
                throw new ArgumentNullException(nameof(moduleBuilder));

            Components.Add(moduleBuilder);

            return this;
        }

        /// <summary>
        ///     Adds a module to the <see cref="Components"/> collection.
        /// </summary>
        /// <remarks>
        ///     When using this method, the module will be created with the default constructor. In order for the module to be valid for execution, <see cref="ModuleBuilder.WithAliases(string[])"/> must be called within <paramref name="configureModule"/>.
        /// </remarks>
        /// <param name="configureModule">An action that extends the fluent API of this type to configure the module.</param>
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        public CommandTreeBuilder AddModule(Action<ModuleBuilder> configureModule)
        {
            var moduleBuilder = new ModuleBuilder();

            configureModule(moduleBuilder);

            return AddModule(moduleBuilder);
        }

        /// <summary>
        ///     Configures an action that runs when a command publishes its result. This action runs after all pipeline actions have been resolved.
        /// </summary>
        /// <remarks>
        ///     The <see cref="IExecuteResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="IExecuteResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        public CommandTreeBuilder AddResultResolver(Action<CallerContext, IExecuteResult, IServiceProvider> resultAction)
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
        ///     The <see cref="IExecuteResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="IExecuteResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        public CommandTreeBuilder AddResultResolver(Func<CallerContext, IExecuteResult, IServiceProvider, ValueTask> resultAction)
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
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        public CommandTreeBuilder AddResultResolver<TResolver>(TResolver resolver)
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
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        public CommandTreeBuilder AddTypeConverter<TConvertable>(Func<CallerContext, IArgument, object?, IServiceProvider, ConvertResult> convertAction)
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
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        public CommandTreeBuilder AddTypeConverter<TConvertable>(Func<CallerContext, IArgument, object?, IServiceProvider, ValueTask<ConvertResult>> convertAction)
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
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        public CommandTreeBuilder AddTypeConverter<TConverter>(TConverter converter)
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
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        public CommandTreeBuilder AddAssembly(Assembly assembly)
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
        /// <returns>The same <see cref="CommandTreeBuilder"/> for call-chaining.</returns>
        public CommandTreeBuilder AddAssemblies(params Assembly[] assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            Assemblies.AddRange(assemblies);
            return this;
        }

        /// <summary>
        ///     Configures the <see cref="RegisterComponentFilter"/> to filter components at registration, based on the provided predicate.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public CommandTreeBuilder WithRegistrationFilter(Func<IComponent, bool> filter)
        {
            RegisterComponentFilter = filter;
            return this;
        }

        /// <summary>
        ///     Builds the current <see cref="CommandTreeBuilder"/> instance into a new <see cref="BuildConfiguration"/>, which is used to create a new instance of <see cref="CommandTree"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="CommandTree"/> built by this builder.</returns>
        public CommandTree Build()
        {
            var configuration = new BuildConfiguration(TypeConverters, NamingRegex, SealModuleDefinitions, RegisterComponentFilter);

            var components = Components.Select(x => x.Build(configuration));

            return new CommandTree(configuration,
                assemblies: Assemblies,
                resolvers: ResultResolvers,
                runtimeComponents: components);
        }
    }
}
