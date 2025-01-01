using System.Diagnostics.CodeAnalysis;

namespace Commands.Builders
{
    /// <summary>
    ///     A configuration builder determining the build process for modules and commands in an <see cref="IComponentTree"/>. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     This builder is able to configure the following:
    ///     <list type="number">
    ///         <item>Defining assemblies through which will be searched to discover modules and commands.</item>
    ///         <item>Defining custom commands that do not appear in the assemblies.</item>
    ///         <item>Registering implementations of <see cref="ResultHandler"/> which define custom result handling.</item>
    ///         <item>Configuring settings for creation of modules and commands.</item>
    ///     </list>
    /// </remarks>
    public interface ITreeBuilder
    {
        /// <summary>
        ///     Gets or sets the component configuration builder for the <see cref="IComponentTree"/>. This configuration is used to determine the build process for modules and commands.
        /// </summary>
        public IConfigurationBuilder Configuration { get; set; }

        /// <summary>
        ///     Gets or sets a collection of <see cref="IComponentBuilder"/>'s that are manually built into implementations of <see cref="IComponent"/> before the registration process runs.
        /// </summary>
        public ICollection<IComponentBuilder> Components { get; set; }

        /// <summary>
        ///     Gets or sets a collection of <see cref="ResultHandler"/> implementations that serve as post-execution handlers.
        /// </summary>
        public ICollection<ResultHandler> Handlers { get; set; }

        /// <summary>
        ///     Gets or sets a collection of types that are to be used to create modules for commands and submodules, implementing <see cref="CommandModule{T}"/> or <see cref="CommandModule"/>.
        /// </summary>
        public ICollection<TypeDefinition> Types { get; set; }

        /// <summary>
        ///     Adds a new <see cref="CommandBuilder"/> to the <see cref="Components"/> collection.
        /// </summary>
        /// <param name="commandBuilder">The builder instance to add to the collection, which will be built into a <see cref="CommandInfo"/> instance that can be executed by the <see cref="IComponentTree"/>.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddCommand(CommandBuilder commandBuilder);

        /// <summary>
        ///     Adds a new <see cref="CommandBuilder"/> to the <see cref="Components"/> collection.
        /// </summary>
        /// <remarks>
        ///     When using this method, a new <see cref="CommandBuilder"/> will be created with its parameterless constructor. In order for the command to be valid for execution, <see cref="ModuleBuilder.WithAliases(string[])"/> must be called within <paramref name="configureCommand"/>.
        /// </remarks>
        /// <param name="configureCommand">An action that extends the fluent API of this type to configure the command.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddCommand(Action<CommandBuilder> configureCommand);

        /// <summary>
        ///     Adds a new <see cref="CommandBuilder"/> to the <see cref="Components"/> collection.
        /// </summary>
        /// <remarks>
        ///     Delegate based commands are able to access the command's scope by implementing <see cref="CommandContext{T}"/> as the first argument in the lambda signature.
        /// </remarks>
        /// <param name="name">The name of the component.</param>
        /// <param name="executionDelegate">The delegate to execute when the provided name of this object is discovered in a search operation.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddCommand(string name, Delegate executionDelegate);

        /// <summary>
        ///     Adds a new <see cref="CommandBuilder"/> to the <see cref="Components"/> collection.
        /// </summary>
        /// <remarks>
        ///     Delegate based commands are able to access the command's scope by implementing <see cref="CommandContext{T}"/> as the first argument in the lambda signature.
        /// </remarks>
        /// <param name="name">The name of the component.</param>
        /// <param name="executionDelegate">The delegate to execute when the provided name of this object is discovered in a search operation.</param>
        /// <param name="aliases">The aliases of the component, excluding the name.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddCommand(string name, Delegate executionDelegate, params string[] aliases);

        /// <summary>
        ///     Adds a new <see cref="ModuleBuilder"/> to the <see cref="Components"/> collection.
        /// </summary>
        /// <param name="moduleBuilder">The builder instance to add to the collection, which will be built into a <see cref="ModuleInfo"/> instance that can contain commands to be executed by the <see cref="IComponentTree"/>.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddModule(ModuleBuilder moduleBuilder);

        /// <summary>
        ///     Adds a new <see cref="ModuleBuilder"/> to the <see cref="Components"/> collection.
        /// </summary>
        /// <remarks>
        ///     When using this method, the module will be created with the default constructor. In order for the module to be valid for execution, <see cref="ModuleBuilder.WithAliases(string[])"/> must be called within <paramref name="configureModule"/>.
        /// </remarks>
        /// <param name="configureModule">An action that extends the fluent API of this type to configure the module.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddModule(Action<ModuleBuilder> configureModule);

        /// <summary>
        ///     Adds a <see cref="Type"/> to the <see cref="Types"/> collection. This method will skip the add operation if the type is already present.
        /// </summary>
        /// <remarks>
        ///     Validations are performed during <see cref="Build"/> to ensure that provided types are a valid module type: A non-nested, non-abstract, non-generic type that implements <see cref="CommandModule"/>; 
        ///     If not, it will be skipped during the registration process, or, if there are static methods marked by <see cref="NameAttribute"/> inside, will register only these components.
        /// </remarks>
        /// <param name="moduleType">A non-nested, non-abstract, non-generic type that implements <see cref="CommandModule"/>.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddType(
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
            Type moduleType);

        /// <summary>
        ///     Adds a <see cref="Type"/> to the <see cref="Types"/> collection. This method will skip the add operation if the type is already present.
        /// </summary>
        /// <remarks>
        ///     Validations are performed during <see cref="Build"/> to ensure that provided types are a valid module type: A non-nested, non-abstract, non-generic type that implements <see cref="CommandModule"/>; 
        ///     If not, it will be skipped during the registration process, or, if there are static methods marked by <see cref="NameAttribute"/> inside, will register only these components.
        /// </remarks>
        /// <typeparam name="T">A non-nested, non-abstract, non-generic type that implements <see cref="CommandModule"/>.</typeparam>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddType<
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        T>()
            where T : class;

        /// <summary>
        ///     Adds a <see cref="Type"/> collection to <see cref="Types"/>. This method will skip the add operation if the type is already present.
        /// </summary>
        /// <param name="types">A collection of <see cref="Type"/> to add to this builder.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddTypes(params Type[] types);

        /// <summary>
        ///     Replaces the <see cref="Types"/> collection with a new collection of types.
        /// </summary>
        /// <param name="types">The collection of <see cref="Type"/> to replace the current collection in this builder with.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder WithTypes(params Type[] types);

        /// <summary>
        ///     Configures an awaitable action to handle failed execution results. This action runs as the last step of execution, when <see cref="IExecuteResult.Success"/> is <see langword="false"/>. 
        /// </summary>
        /// <remarks>
        ///     To handle both failed and successful results, use <see cref="AddResultHandler(ResultHandler)"/> with an implementation of <see cref="ResultHandler"/> or <see cref="ResultHandler{T}"/>.
        /// </remarks>
        /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddResultHandler(Func<ICallerContext, IExecuteResult, IServiceProvider, ValueTask> resultAction);

        /// <summary>
        ///     Configures an awaitable action to handle failed execution results. This action runs as the last step of execution, when <see cref="IExecuteResult.Success"/> is <see langword="false"/> and the <see cref="ICallerContext"/> matches <typeparamref name="T"/>. 
        /// </summary>
        /// <remarks>
        ///     To handle both failed and successful results, use <see cref="AddResultHandler(ResultHandler)"/> with an implementation of <see cref="ResultHandler"/> or <see cref="ResultHandler{T}"/>.
        /// </remarks>
        /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddResultHandler<T>(Func<T, IExecuteResult, IServiceProvider, ValueTask> resultAction)
            where T : class, ICallerContext;

        /// <summary>
        ///     Adds an implementation of <see cref="ResultHandler"/> to <see cref="Handlers"/>.
        /// </summary>
        /// <param name="resolver">The implementation of <see cref="ResultHandler"/> to add.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddResultHandler(ResultHandler resolver);

        /// <summary>
        ///     Configures the <see cref="Configuration"/> property with modified values through the <paramref name="configure"/> action.
        /// </summary>
        /// <param name="configure">An action that runs over the configuration to create .</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder ConfigureComponents(Action<IConfigurationBuilder> configure);

        /// <summary>
        ///     Builds the current <see cref="ITreeBuilder"/> instance into a new <see cref="IComponentTree"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="IComponentTree"/> built by this builder.</returns>
        public IComponentTree Build();
    }
}
