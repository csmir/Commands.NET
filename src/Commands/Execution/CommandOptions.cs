namespace Commands;

/// <summary>
///     A set of options for handling command queries and determining the workflow in the command scope. This class cannot be inherited.
/// </summary>
public sealed class CommandOptions
{
    // A reference to the component manager that called the command, if any.
    internal ComponentManager? Manager;

    /// <summary>
    ///     Gets or sets the services for running the request.
    /// </summary>
    /// <remarks>
    ///     Default: <see cref="EmptyServiceProvider.Instance" />
    /// </remarks>
    public IServiceProvider Services { get; set; } = EmptyServiceProvider.Instance;

    /// <summary>
    ///     Gets or sets a token that can be provided from a <see cref="CancellationTokenSource"/> and later used to cancel asynchronous execution
    /// </summary>
    /// <remarks>
    ///     Default: <see langword="default"/>
    /// </remarks> 
    public CancellationToken CancellationToken { get; set; } = default;

    /// <summary>
    ///     Gets or sets the approach to asynchronousity in command execution.
    ///     This value drastically changes the expected behavior of executing a command:
    ///     <list type="bullet">
    ///         <item>
    ///             <see langword="false"/> is the default value and tells the pipeline to finish executing before returning control to the caller. 
    ///             This ensures that the execution will fully finish whether it failed or not, before allowing another command to be executed.
    ///         </item>
    ///         <item>
    ///             When marked as <see langword="true"/>, the execution will return immediately after the entrypoint is called, slipping thread for the rest of execution.
    ///         </item>
    ///     </list>
    /// </summary>
    /// <remarks>
    ///     When changing this setting to <see langword="true"/>, the following should be checked for thread-safety:
    ///     <list type="number">
    ///         <item>
    ///             Services, specifically those created as singleton or scoped to anything but a single command.
    ///         </item>
    ///         <item>
    ///             Implementations of <see cref="TypeParser"/>, <see cref="TypeParser{T}"/>, <see cref="ConditionAttribute{T}"/> or <see cref="ConditionAttribute{TEval, TContext}"/>.
    ///         </item>
    ///         <item>
    ///             Mutable collections and objects with shared access. <see cref="ComponentCollection"/> implementations are concurrently accessible, not needing additional effort to make thread-safe.
    ///         </item>
    ///     </list>
    ///     For ensuring thread safety in any of the above situations, it is important to know what this actually means. 
    ///     <br/>
    ///     For more information, consider reading this article: <see href="https://learn.microsoft.com/en-us/dotnet/standard/threading/managed-threading-best-practices"/>
    /// </remarks>
    public bool AsynchronousExecution { get; set; } = false;

    /// <summary>
    ///     Gets or sets whether the defined <see cref="ConditionAttribute{T}"/>'s for this execution should be ran.
    /// </summary>
    /// <remarks>
    ///     Default: <see langword="false"/>
    /// </remarks> 
    public bool SkipConditions { get; set; } = false;

    /// <summary>
    ///     Gets or sets the separator used to join remaining arguments in a command.
    /// </summary>
    /// <remarks>
    ///     Default: <c>' '</c>
    /// </remarks> 
    public char RemainderSeparator { get; set; } = ' ';

    /// <inheritdoc cref="IServiceProvider" />
    /// <remarks>
    ///     This class implements the <see cref="IServiceProvider"/> interface and returns <see langword="null"/> for all service requests.
    /// </remarks>
    public sealed class EmptyServiceProvider : IServiceProvider
    {
        private static readonly EmptyServiceProvider _i = new();

        internal static EmptyServiceProvider Instance
            => _i;

        /// <inheritdoc />
        public object? GetService(Type serviceType)
            => null;
    }
}
