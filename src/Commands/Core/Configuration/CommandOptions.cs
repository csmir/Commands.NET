using Commands.Conditions;

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
        ///     Gets or sets the approach to asynchronousity in command execution.
        /// </summary>
        /// <remarks>
        ///     If set to <see cref="AsyncMode.Await"/>, the manager will wait for a command to finish before allowing another to be executed.
        ///     If set to <see cref="AsyncMode.Async"/>, the manager will seperate the command execution from the entry stack, and slip it to another if necessary. 
        ///     Only change this value if you have read the documentation of <see cref="Commands.AsyncMode"/> and understand the definitions.
        ///     <br/>
        ///     <br/>
        ///     Default: <see cref="AsyncMode.Default"/> (await).
        /// </remarks>
        public AsyncMode AsyncMode { get; set; } = AsyncMode.Default;

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
        ///     Gets or sets whether the defined <see cref="PostconditionAttribute{T}"/>'s for this execution should be ran.
        /// </summary>
        /// <remarks>
        ///     Default: <see langword="false"/>
        /// </remarks>
        public bool SkipPostconditions { get; set; } = false;

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
            {
                get
                {
                    return _i.Value;
                }
            }

            /// <inheritdoc />
            public object? GetService(Type serviceType)
            {
                return null;
            }
        }
    }
}
