using System.Reflection;
using System.Text.RegularExpressions;

namespace Commands.Builders
{
    /// <inheritdoc cref="ITreeBuilder" />
    public sealed class ComponentTreeBuilder : ITreeBuilder
    {
        const string DEFAULT_NAMEPATTERN = @"^[a-z0-9_-]*$";

        /// <inheritdoc />
        public IConfigurationBuilder Configuration { get; set; } = new ComponentConfigurationBuilder();

        /// <inheritdoc />
        public ICollection<IComponentBuilder> Components { get; set; } = [];

        /// <inheritdoc />
        public ICollection<Assembly> Assemblies { get; set; } = [Assembly.GetEntryAssembly()!];

        /// <inheritdoc />
        public ICollection<ResultHandler> Handlers { get; set; } = [];

        /// <inheritdoc />
        public Func<IComponent, bool> ComponentRegistrationFilter { get; set; } = _ => true;

        /// <inheritdoc />
        public bool MakeModulesReadonly { get; set; } = false;

        /// <inheritdoc />
        public string? NamingPattern { get; set; } = DEFAULT_NAMEPATTERN;

        /// <inheritdoc />
        public ITreeBuilder AddCommand(CommandBuilder commandBuilder)
        {
            if (commandBuilder == null)
                throw new ArgumentNullException(nameof(commandBuilder));

            Components.Add(commandBuilder);

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder AddCommand(Action<CommandBuilder> configureCommand)
        {
            var commandBuilder = new CommandBuilder();

            configureCommand(commandBuilder);

            return AddCommand(commandBuilder);
        }

        /// <inheritdoc />
        public ITreeBuilder AddCommand(string name, Delegate executionDelegate)
        {
            var commandBuilder = new CommandBuilder(name, [], executionDelegate);

            return AddCommand(commandBuilder);
        }

        /// <inheritdoc />
        public ITreeBuilder AddCommand(string name, Delegate executionDelegate, params string[] aliases)
        {
            var commandBuilder = new CommandBuilder(name, aliases, executionDelegate);

            return AddCommand(commandBuilder);
        }

        /// <inheritdoc />
        public ITreeBuilder AddModule(ModuleBuilder moduleBuilder)
        {
            if (moduleBuilder == null)
                throw new ArgumentNullException(nameof(moduleBuilder));

            Components.Add(moduleBuilder);

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder AddModule(Action<ModuleBuilder> configureModule)
        {
            var moduleBuilder = new ModuleBuilder();

            configureModule(moduleBuilder);

            return AddModule(moduleBuilder);
        }

        /// <inheritdoc />
        public ITreeBuilder AddResultHandler(Action<ICallerContext, IExecuteResult, IServiceProvider> resultAction)
        {
            if (resultAction == null)
                throw new ArgumentNullException(nameof(resultAction));

            Handlers.Add(new DelegateResultHandler(resultAction));

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder AddResultResolver(Func<ICallerContext, IExecuteResult, IServiceProvider, ValueTask> resultAction)
        {
            if (resultAction == null)
                throw new ArgumentNullException(nameof(resultAction));

            Handlers.Add(new AsyncDelegateResultHandler(resultAction));

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder AddResultResolver(ResultHandler resolver)
        {
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            Handlers.Add(resolver);

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder AddAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            if (Assemblies.Contains(assembly))
                return this;

            Assemblies.Add(assembly);

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder AddAssemblies(params Assembly[] assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            foreach (var assembly in assemblies)
                AddAssembly(assembly);

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder WithRegistrationFilter(Func<IComponent, bool> filter)
        {
            ComponentRegistrationFilter = filter;

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder ConfigureComponents(Action<IConfigurationBuilder> configure)
        {
            Configuration ??= new ComponentConfigurationBuilder();

            configure(Configuration);

            return this;
        }

        /// <inheritdoc />
        public IComponentTree Build()
        {
            Configuration.Properties["ComponentRegistrationFilter"] = ComponentRegistrationFilter;

            // Set property with null value, when a key is set but no value is provided.
            if (MakeModulesReadonly)
                Configuration.Properties["ReadOnlyModuleDefinitions"] = null;

            if (!string.IsNullOrEmpty(NamingPattern))
                Configuration.Properties["NamingPattern"] = new Regex(NamingPattern);

            var configuration = Configuration.Build();

            var components = Components.Select(x => x.Build(configuration));

            return new ComponentTree(configuration,
                assemblies: Assemblies,
                resolvers: Handlers,
                runtimeComponents: components);
        }
    }
}
