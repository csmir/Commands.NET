using Commands.Reflection;

namespace Commands
{
    /// <summary>
    ///     Represents the builder of a module that can contain commands and sub-modules. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     This class is used to build a module that can contain commands and sub-modules. The module can be built using the <see cref="Build(CommandConfiguration)"/> method, which returns a <see cref="ModuleInfo"/> instance. 
    ///     This reflection container is not type-locked and does not have an instance. It is used to run delegate commands in a tree-like structure.
    /// </remarks>
    public sealed class ModuleBuilder : IComponentBuilder
    {
        /// <inheritdoc />
        public string[] Aliases { get; }

        /// <summary>
        ///     Gets the name of the module. This is the primary alias of the module.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets or sets a collection of components that are added to the module. This collection is used to build the module into a <see cref="ModuleInfo"/> object.
        /// </summary>
        public List<IComponentBuilder> Components { get; set; } = [];

        /// <summary>
        ///     Creates a new instance of <see cref="ModuleBuilder"/> with the specified name and aliases.
        /// </summary>
        /// <param name="name">The primary alias of the module.</param>
        /// <param name="aliases">All remaining aliases of the module.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided aliases or name are null.</exception>
        public ModuleBuilder(string name, string[] aliases)
        {
            if (aliases == null)
                throw new ArgumentNullException(nameof(aliases));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            aliases = new string[] { name }
                .Concat(aliases)
                .Distinct()
                .ToArray();

            Name = name;
            Aliases = aliases;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ModuleBuilder"/> with the specified name and aliases.
        /// </summary>
        /// <param name="name">The primary alias of the module.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided aliases or name are null.</exception>
        public ModuleBuilder(string name)
            : this(name, [])
        {
        }

        /// <summary>
        ///     Adds a command to the <see cref="Components"/> collection.
        /// </summary>
        /// <param name="commandBuilder">The builder instance to add to the collection, which will be built into a <see cref="CommandInfo"/> instance that can be executed by the <see cref="CommandManager"/>.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided builder is <see langword="null"/>.</exception>
        public ModuleBuilder AddCommand(CommandBuilder commandBuilder)
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
        ///     Delegate based commands are able to access the command's scope by implementing <see cref="CommandContext{T}"/> as the first argument in the lambda signature.
        /// </remarks>
        /// <param name="executionDelegate">The delegate to execute when the root module of this object is discovered in a search operation.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ModuleBuilder AddCommand(Delegate executionDelegate)
        {
            var commandBuilder = new CommandBuilder(executionDelegate);

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
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ModuleBuilder AddCommand(string name, Delegate executionDelegate)
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
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ModuleBuilder AddCommand(string name, Delegate executionDelegate, params string[] aliases)
        {
            var commandBuilder = new CommandBuilder(name, aliases, executionDelegate);

            return AddCommand(commandBuilder);
        }

        /// <summary>
        ///     Adds a module to the <see cref="Components"/> collection.
        /// </summary>
        /// <param name="moduleBuilder">The builder instance to add to the collection, which will be built into a <see cref="ModuleInfo"/> instance that can contain commands to be executed by the <see cref="CommandManager"/>.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided builder is <see langword="null"/>.</exception>
        public ModuleBuilder AddModule(ModuleBuilder moduleBuilder)
        {
            if (moduleBuilder == null)
                throw new ArgumentNullException(nameof(moduleBuilder));

            Components.Add(moduleBuilder);

            return this;
        }

        /// <summary>
        ///     Adds a module to the <see cref="Components"/> collection.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <param name="configureModule">An action that extends the fluent API of this type to configure the module.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ModuleBuilder AddModule(string name, Action<ModuleBuilder> configureModule)
        {
            var moduleBuilder = new ModuleBuilder(name, []);

            configureModule(moduleBuilder);

            return AddModule(moduleBuilder);
        }

        /// <summary>
        ///     Adds a module to the <see cref="Components"/> collection.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <param name="configureModule">An action that extends the fluent API of this type to configure the module.</param>        
        /// <param name="aliases">The aliases of the component, excluding the name.</param>
        /// <returns>The same <see cref="ConfigurationBuilder"/> for call-chaining.</returns>
        public ModuleBuilder AddModule(string name, Action<ModuleBuilder> configureModule, params string[] aliases)
        {
            var moduleBuilder = new ModuleBuilder(name, aliases);

            configureModule(moduleBuilder);

            return AddModule(moduleBuilder);
        }

        /// <summary>
        ///     Adds a new (sub)module to the module. This is a subcommand group, serving as a named container for subcommands and submodules.
        /// </summary>
        /// <param name="configuration">The configuration that should be used to determine the validity of the provided module.</param>
        /// <param name="root">The root module of this (sub)module. Can be left null, but it will affect how the module is visually formatted in the debugger and by calling the ToString() override on the returned type.</param>
        /// <returns>The same <see cref="ModuleBuilder"/> for call-chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown when any of the aliases of the module to be built do not match <see cref="CommandConfiguration.NamingRegex"/>.</exception>
        public ModuleInfo Build(CommandConfiguration configuration, ModuleInfo? root = null)
        {
            foreach (var alias in Aliases)
            {
                if (!configuration.NamingRegex.IsMatch(alias))
                    throw new InvalidOperationException($"The alias of must match the filter provided in the {nameof(CommandConfiguration.NamingRegex)} of the {nameof(CommandConfiguration)}.");
            }

            var moduleInfo = new ModuleInfo(root, Aliases);

            foreach (var component in Components)
            {
                if (component is ModuleBuilder moduleBuilder)
                {
                    var subModuleInfo = moduleBuilder.Build(configuration, moduleInfo);

                    moduleInfo.Components.Add(subModuleInfo);
                }
                else
                {
                    var commandInfo = component.Build(configuration);

                    moduleInfo.Components.Add(commandInfo);
                }
            }

            moduleInfo.SortScores();

            return moduleInfo;
        }

        /// <inheritdoc />
        public ISearchable Build(CommandConfiguration configuration)
        {
            return Build(configuration, null);
        }
    }
}
