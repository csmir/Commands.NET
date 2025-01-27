using Commands.Conditions;
using Commands.Parsing;

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
    ///     Gets or sets whether the defined <see cref="ExecuteConditionAttribute{T}"/>'s for this execution should be ran.
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
