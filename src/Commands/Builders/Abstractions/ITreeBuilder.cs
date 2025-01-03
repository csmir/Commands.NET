namespace Commands.Builders;

/// <summary>
///     A configuration builder determining the build process for groups and commands in an <see cref="IComponentTree"/>. This class cannot be inherited.
/// </summary>
/// <remarks>
///     This builder is able to configure the following:
///     <list type="number">
///         <item>Defining assemblies through which will be searched to discover groups and commands.</item>
///         <item>Defining custom commands that do not appear in the assemblies.</item>
///         <item>Registering implementations of <see cref="ResultHandler"/> which define custom result handling.</item>
///         <item>Configuring settings for creation of groups and commands.</item>
///     </list>
/// </remarks>
public interface ITreeBuilder
{
    /// <summary>
    ///     Gets or sets the component configuration builder for the <see cref="IComponentTree"/>. This configuration is used to determine the build process for groups and commands.
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
    ///     Gets or sets a collection of types that are to be used to create groups for commands and subgroups, implementing <see cref="CommandModule{T}"/> or <see cref="CommandModule"/>.
    /// </summary>
    public ICollection<DynamicType> Types { get; set; }

    /// <summary>
    ///     Adds a new <see cref="CommandBuilder"/> to the <see cref="Components"/> collection.
    /// </summary>
    /// <param name="commandBuilder">The builder instance to add to the collection, which will be built into a <see cref="Command"/> instance that can be executed by the <see cref="IComponentTree"/>.</param>
    /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
    public ITreeBuilder AddCommand(CommandBuilder commandBuilder);

    /// <summary>
    ///     Adds a new <see cref="CommandBuilder"/> to the <see cref="Components"/> collection.
    /// </summary>
    /// <remarks>
    ///     When using this method, a new <see cref="CommandBuilder"/> will be created with its parameterless constructor. In order for the command to be valid for execution, <see cref="CommandGroupBuilder.WithNames(string[])"/> must be called within <paramref name="configureCommand"/>.
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
    /// <param name="names">The names of the component, excluding the name.</param>
    /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
    public ITreeBuilder AddCommand(string name, Delegate executionDelegate, params string[] names);

    /// <summary>
    ///     Adds a new <see cref="CommandGroupBuilder"/> to the <see cref="Components"/> collection.
    /// </summary>
    /// <param name="groupBuilder">The builder instance to add to the collection, which will be built into a <see cref="CommandGroup"/> instance that can contain commands to be executed by the <see cref="IComponentTree"/>.</param>
    /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
    public ITreeBuilder AddCommandGroup(CommandGroupBuilder groupBuilder);

    /// <summary>
    ///     Adds a new <see cref="CommandGroupBuilder"/> to the <see cref="Components"/> collection.
    /// </summary>
    /// <remarks>
    ///     When using this method, the group will be created with the default constructor. In order for the group to be valid for execution, <see cref="CommandGroupBuilder.WithNames(string[])"/> must be called within <paramref name="configureGroup"/>.
    /// </remarks>
    /// <param name="configureGroup">An action that extends the fluent API of this type to configure the group.</param>
    /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
    public ITreeBuilder AddCommandGroup(Action<CommandGroupBuilder> configureGroup);

    /// <summary>
    ///     Adds a <see cref="Type"/> to the <see cref="Types"/> collection. This method will skip the add operation if the type is already present.
    /// </summary>
    /// <remarks>
    ///     Validations are performed during <see cref="Build"/> to ensure that provided types are a valid module type: A non-nested, non-abstract, non-generic type that implements <see cref="CommandModule"/>; 
    ///     If not, it will be skipped during the registration process.
    /// </remarks>
    /// <param name="groupType">A non-nested, non-abstract, non-generic type that implements <see cref="CommandModule"/>.</param>
    /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
    public ITreeBuilder AddType(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type groupType);

    /// <summary>
    ///     Adds a <see cref="Type"/> to the <see cref="Types"/> collection. This method will skip the add operation if the type is already present.
    /// </summary>
    /// <remarks>
    ///     Validations are performed during <see cref="Build"/> to ensure that provided types are a valid module type: A non-nested, non-abstract, non-generic type that implements <see cref="CommandModule"/>; 
    ///     If not, it will be skipped during the registration process.
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
    public ITreeBuilder AddResultHandler(Action<ICallerContext, IExecuteResult, IServiceProvider> resultAction);

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
    ///     Configures an action to handle failed execution results. This action runs as the last step of execution, when <see cref="IExecuteResult.Success"/> is <see langword="false"/> and the <see cref="ICallerContext"/> matches <typeparamref name="T"/>. 
    /// </summary>
    /// <remarks>
    ///     To handle both failed and successful results, use <see cref="AddResultHandler(ResultHandler)"/> with an implementation of <see cref="ResultHandler"/> or <see cref="ResultHandler{T}"/>.
    /// </remarks>
    /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
    /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
    public ITreeBuilder AddResultHandler<T>(Action<T, IExecuteResult, IServiceProvider> resultAction)
        where T : class, ICallerContext;

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
