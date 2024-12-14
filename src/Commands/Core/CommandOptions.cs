using Commands.Conditions;
using Commands.Converters;

namespace Commands
{
    /// <summary>
    ///     A set of options for handling command queries and determining the workflow in the command scope.
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
        ///     Gets or sets an ID that can be used to trace a command execution task through the pipeline. This ID should be unique per execution.
        /// </summary>
        /// <remarks>
        ///     Default: <see cref="Guid.NewGuid"/>
        /// </remarks>
        public Guid TraceId { get; set; } = Guid.NewGuid();

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
        ///             Implementations of <see cref="TypeConverterBase"/>, <see cref="TypeConverterBase{T}"/>, <see cref="PreconditionAttribute{T}"/> and <see cref="PostconditionAttribute{T}"/>.
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
        ///     Gets or sets whether the defined <see cref="PostconditionAttribute{T}"/>'s for this execution should be ran.
        /// </summary>
        /// <remarks>
        ///     Default: <see langword="true"/>
        /// </remarks>
        public bool SkipPostconditions { get; set; } = true;

        /// <summary>
        ///     Gets or sets whether the defined <see cref="PreconditionAttribute{T}"/>'s for this execution should be ran.
        /// </summary>
        /// <remarks>
        ///     Default: <see langword="false"/>
        /// </remarks>
        public bool SkipPreconditions { get; set; } = false;

        /// <summary>
        ///     Gets or sets the comparer used to match command names and named arguments.
        /// </summary>
        /// <remarks>
        ///     Default: <see cref="StringComparer.OrdinalIgnoreCase"/>
        /// </remarks>
        public StringComparer MatchComparer { get; set; } = StringComparer.OrdinalIgnoreCase;

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
