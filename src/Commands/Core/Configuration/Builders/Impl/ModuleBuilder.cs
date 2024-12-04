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
        /// <summary>
        ///     Gets the name of the module. This is the primary alias of the module.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets all aliases of the module, including its name. This is used to identify the module in the command execution pipeline.
        /// </summary>
        public string[] Aliases { get; }

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

        internal ModuleBuilder(string[] aliases)
        {
            Name = aliases[0];
            Aliases = aliases;
        }

        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the module.
        /// </summary>
        /// <param name="command">The command builder to add.</param>
        /// <returns>The same <see cref="ModuleBuilder"/> for call-chaining.</returns>
        public ModuleBuilder AddCommand(CommandBuilder command)
        {
            Components.Add(command);

            return this;
        }

        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the module.
        /// </summary>
        /// <param name="name">The name of the command to add. This is the primary alias.</param>
        /// <param name="commandAction">The action of the command. Delegate commands are adviced to set the first parameter to be <see cref="CommandContext{T}"/>, which holds scope and execution information of the created command during its execution.</param>
        /// <param name="aliases">The aliases of the command.</param>
        /// <returns>The same <see cref="ModuleBuilder"/> for call-chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the aliases, execution delegate or name are null.</exception>
        public ModuleBuilder AddCommand(string name, Delegate commandAction, params string[] aliases)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            if (commandAction == null)
                throw new ArgumentNullException(nameof(commandAction));

            if (aliases == null)
                throw new ArgumentNullException(nameof(aliases));

            aliases = new string[] { name }
                .Concat(aliases)
                .Distinct()
                .ToArray();

            return AddCommand(new CommandBuilder(aliases, commandAction));
        }

        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the module.
        /// </summary>
        /// <param name="name">The name of the command to add. This is the primary alias.</param>
        /// <param name="commandAction">The action of the command. Delegate commands are adviced to set the first parameter to be <see cref="CommandContext{T}"/>, which holds scope and execution information of the created command during its execution.</param>
        /// <returns>The same <see cref="ModuleBuilder"/> for call-chaining.</returns>
        public ModuleBuilder AddCommand(string name, Delegate commandAction)
        {
            return AddCommand(name, commandAction, []);
        }

        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the module.
        /// </summary>
        /// <remarks>
        ///     This command variant serves as a default overload for a module. 
        ///     When the module name is provided but no subcommand name, this delegate is discovered for execution, and will execute if the parameters match.
        /// </remarks>
        /// <param name="commandAction">The action of the command. Delegate commands are adviced to set the first parameter to be <see cref="CommandContext{T}"/>, which holds scope and execution information of the created command during its execution.</param>
        /// <returns>The same <see cref="ModuleBuilder"/> for call-chaining.</returns>
        public ModuleBuilder AddCommand(Delegate commandAction)
        {
            return AddCommand(new CommandBuilder(commandAction));
        }

        /// <summary>
        ///     Adds a new (sub)module to the module. This is a subcommand group, serving as a named container for subcommands and submodules.
        /// </summary>
        /// <param name="module">The module builder to add.</param>
        /// <returns>The same <see cref="ModuleBuilder"/> for call-chaining.</returns>
        public ModuleBuilder AddModule(ModuleBuilder module)
        {
            Components.Add(module);

            return this;
        }

        /// <summary>
        ///     Adds a new (sub)module to the module. This is a subcommand group, serving as a named container for subcommands and submodules.
        /// </summary>
        /// <param name="name">The name of the module. This is the primary alias.</param>
        /// <param name="aliases">The aliases of the module.</param>
        /// <param name="moduleConfiguration">An action which is ran over the to-be created submodule to configure it with additional components.</param>
        /// <returns>The same <see cref="ModuleBuilder"/> for call-chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the aliases or name are null.</exception>
        public ModuleBuilder AddModule(string name, Action<ModuleBuilder> moduleConfiguration, params string[] aliases)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            if (aliases == null)
                throw new ArgumentNullException(nameof(aliases));

            aliases = new string[] { name }
                .Concat(aliases)
                .Distinct()
                .ToArray();

            var subModule = new ModuleBuilder(aliases);

            moduleConfiguration(subModule);

            return AddModule(subModule);
        }

        /// <summary>
        ///     Adds a new (sub)module to the module. This is a subcommand group, serving as a named container for subcommands and submodules.
        /// </summary>
        /// <param name="name">The name of the module. This is the primary alias.</param>
        /// <param name="moduleConfiguration">An action which is ran over the to-be created submodule to configure it with additional components.</param>
        /// <returns>The same <see cref="ModuleBuilder"/> for call-chaining.</returns>
        public ModuleBuilder AddModule(string name, Action<ModuleBuilder> moduleConfiguration)
        {
            return AddModule(name, moduleConfiguration, []);
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
