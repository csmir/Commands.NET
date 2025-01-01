using System.Diagnostics.CodeAnalysis;

namespace Commands.Builders
{
    /// <summary>
    ///     A builder model for a tree of components. This class cannot be inherited.
    /// </summary>
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
        public ITreeBuilder AddCommand(CommandBuilder commandBuilder)
        {
            Assert.NotNull(commandBuilder, nameof(commandBuilder));

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
            Assert.NotNull(moduleBuilder, nameof(moduleBuilder));

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
            Assert.NotNull(moduleType, nameof(moduleType));

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

        /// <inheritdoc />
#if NET8_0_OR_GREATER
        [UnconditionalSuppressMessage("AotAnalysis", "IL2072", Justification = "The types are supplied from user-facing implementation, it is up to the user to ensure that these types are available in AOT context.")]
#endif
        public ITreeBuilder AddTypes(params Type[] types)
        {
            // We cannot add the range to the collection immediately, because we need AddType to infer DynamicallyAccessedMemberTypes.All
            foreach (var type in types)
                AddType(type);

            return this;
        }

        /// <inheritdoc />
#if NET8_0_OR_GREATER
        [UnconditionalSuppressMessage("AotAnalysis", "IL2072", Justification = "The types are supplied from user-facing implementation, it is up to the user to ensure that these types are available in AOT context.")]
#endif
        public ITreeBuilder WithTypes(params Type[] types)
        {
            // We cannot reassign the collection, because we need AddType to infer DynamicallyAccessedMemberTypes.All
            Types = [];

            foreach (var type in types)
                AddType(type);

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder AddResultHandler(Func<ICallerContext, IExecuteResult, IServiceProvider, ValueTask> resultAction)
        {
            Assert.NotNull(resultAction, nameof(resultAction));

            Handlers.Add(new DelegateResultHandler(resultAction));

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder AddResultHandler<T>(Func<T, IExecuteResult, IServiceProvider, ValueTask> resultAction)
            where T : class, ICallerContext
        {
            Assert.NotNull(resultAction, nameof(resultAction));

            Handlers.Add(new DelegateResultHandler<T>(resultAction));

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder AddResultHandler(ResultHandler resolver)
        {
            Assert.NotNull(resolver, nameof(resolver));

            Handlers.Add(resolver);

            return this;
        }

        /// <inheritdoc />
        public ITreeBuilder ConfigureComponents(Action<IConfigurationBuilder> configure)
        {
            Assert.NotNull(configure, nameof(configure));

            Configuration ??= new ComponentConfigurationBuilder();

            configure(Configuration);

            return this;
        }

        /// <inheritdoc />
        public IComponentTree Build()
        {
            var configuration = Configuration.Build();

            var components = configuration.BuildModules([.. Types], null, false)
                .Concat(Components.Select(x => x.Build(configuration)));

            return new ComponentTree(components, [.. Handlers]);
        }
    }
}
