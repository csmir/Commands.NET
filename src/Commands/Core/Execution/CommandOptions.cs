using Commands.Conditions;
using Commands.Conversion;

namespace Commands
{
    /// <summary>
    ///     A set of options for handling command queries and determining the workflow in the command scope. This class cannot be inherited.
    /// </summary>
    public sealed class CommandOptions
    {
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
        ///     The asynchronous execution approach drastically changes the expected behavior of executing a command:
        ///     <list type="bullet">
        ///         <item>
        ///             <see langword="false"/> is the default value and tells the pipeline to finish executing before returning control to the caller. 
        ///             This ensures that the execution will fully finish executing, whether it failed or not, before allowing another to be executed.
        ///         </item>
        ///         <item>
        ///             <see langword="true"/> is a value to be treated with care. 
        ///             Instead of waiting for the full execution before returning control, the execution will return immediately after the entrypoint is called, slipping thread for the rest of execution. 
        ///             When more than one input source is expected to be handled, this is generally the advised method of execution. 
        ///         </item>
        ///     </list>
        /// </summary>
        /// <remarks>
        ///     When changing this setting, the following should be checked for thread-safety:
        ///     <list type="number">
        ///         <item>
        ///             Services, specifically those created as singleton or scoped to anything but a single command.
        ///         </item>
        ///         <item>
        ///             Implementations of <see cref="TypeParser"/>, <see cref="TypeParser{T}"/> and <see cref="ConditionAttribute"/>, <see cref="ConditionAttribute{T}"/>.
        ///         </item>
        ///         <item>
        ///             Generic collections and objects with shared access.
        ///         </item>
        ///     </list>
        ///     For ensuring thread safety in any of the above situations, it is important to know what this actually means. 
        ///     <br/>
        ///     For more information, consider reading this article: <see href="https://learn.microsoft.com/en-us/dotnet/standard/threading/managed-threading-best-practices"/>
        /// </remarks>
        public bool DoAsynchronousExecution { get; set; } = false;

        /// <summary>
        ///     Gets or sets whether the defined <see cref="ConditionAttribute{T}"/>'s for this execution should be ran.
        /// </summary>
        public bool SkipConditions { get; set; } = false;

        /// <summary>
        ///     Gets or sets the separator used to join remaining arguments in a command.
        /// </summary>
        public char RemainderSeparator { get; set; } = ' ';

        /// <summary>
        ///     Gets or sets the comparer used to match command names and named arguments.
        /// </summary>
        public StringComparer Comparer { get; set; } = StringComparer.OrdinalIgnoreCase;

        /// <inheritdoc cref="IServiceProvider" />
        /// <remarks>
        ///     This class implements the <see cref="IServiceProvider"/> interface and returns <see langword="null"/> for all service requests.
        /// </remarks>
        public sealed class EmptyServiceProvider : IServiceProvider
        {
            private static readonly Lazy<EmptyServiceProvider> _i = new();

            internal static EmptyServiceProvider Instance
                => _i.Value;

            /// <inheritdoc />
            public object? GetService(Type serviceType)
                => null;
        }
    }
}
