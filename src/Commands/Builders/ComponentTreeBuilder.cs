using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Commands.Builders
{
    /// <inheritdoc cref="ITreeBuilder" />
    public sealed class ComponentTreeBuilder : ITreeBuilder
    {
        /// <inheritdoc />
        public IConfigurationBuilder Configuration { get; set; } = new ComponentConfigurationBuilder();

        /// <inheritdoc />
        public ICollection<IComponentBuilder> Components { get; set; } = [];

        /// <inheritdoc />
        public ICollection<ResultHandler> Handlers { get; set; } = [];

        /// <inheritdoc />
        public ICollection<TypeDefinition> Types { get; set; } = [];

        /// <inheritdoc />
        /// <remarks>
        ///     This property is set to <see langword="false"/> by default. If set to <see langword="true"/>, all modules in <see cref="Components"/> and <see cref="Types"/> be set to read-only after the build process.
        /// </remarks>
        public bool MakeModulesReadonly { get; set; } = false;

        /// <inheritdoc />
        /// <remarks>
        ///     This property is set to <c>@"^[a-z0-9_-]*$"</c> by default.
        /// </remarks>
        public string? NamingPattern { get; set; } = @"^[a-z0-9_-]*$";

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
        public ITreeBuilder AddType(
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
# endif
            Type moduleType)
        {
            if (Types.Contains(moduleType))
                return this;

            Types.Add(moduleType);

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder AddType<
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
            T>()
            where T : class
            => AddType(typeof(T));

#pragma warning disable IL2072
        /// <inheritdoc />
        public ITreeBuilder AddTypes(params Type[] types)
        {
            // We cannot add the range to the collection immediately, because we need AddType to infer DynamicallyAccessedMemberTypes.All
            foreach (var type in types)
                AddType(type);

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder WithTypes(params Type[] types)
        {
            // We cannot reassign the collection, because we need AddType to infer DynamicallyAccessedMemberTypes.All
            Types = [];

            foreach (var type in types)
                AddType(type);

            return this;
        }
#pragma warning restore IL2072

        /// <inheritdoc />
        public ITreeBuilder AddResultHandler(Func<ICallerContext, IExecuteResult, IServiceProvider, ValueTask> resultAction)
        {
            if (resultAction == null)
                throw new ArgumentNullException(nameof(resultAction));

            Handlers.Add(new DelegateResultHandler(resultAction));

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder AddResultHandler<T>(Func<T, IExecuteResult, IServiceProvider, ValueTask> resultAction)
            where T : class, ICallerContext
        {
            if (resultAction == null)
                throw new ArgumentNullException(nameof(resultAction));

            Handlers.Add(new DelegateResultHandler<T>(resultAction));

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder AddResultHandler(ResultHandler resolver)
        {
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            Handlers.Add(resolver);

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder WithRegistrationFilter(Func<IComponent, bool> filter)
        {
            Configuration.Properties[ConfigurationPropertyDefinitions.ComponentRegistrationFilterExpression] = filter;

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
            Configuration.Properties[ConfigurationPropertyDefinitions.MakeModulesReadonly] = MakeModulesReadonly;

            if (!string.IsNullOrEmpty(NamingPattern))
                Configuration.Properties[ConfigurationPropertyDefinitions.NameValidationExpression] = new Regex(NamingPattern);

            var configuration = Configuration.Build();

            var components = configuration.GetModules([.. Types], null, false)
                .Concat(Components.Select(x => x.Build(configuration)));

            return new ComponentTree(components, [.. Handlers]);
        }
    }
}
