using Commands.Conditions;

namespace Commands;

/// <summary>
///     A set of options for handling command queries and determining the workflow in the command scope. This class cannot be inherited.
/// </summary>
public sealed class ExecutionOptions
{
    // A reference to the component manager that called the command, if any.
    internal ComponentProvider? ComponentProvider;

    /// <summary>
    ///     Gets or sets the services for running the request.
    /// </summary>
    public IServiceProvider ServiceProvider { get; set; } = EmptyServiceProvider.Instance;

    /// <summary>
    ///     Gets or sets a token that can be provided from a <see cref="CancellationTokenSource"/> and later used to cancel asynchronous execution
    /// </summary>
    public CancellationToken CancellationToken { get; set; } = default;

    /// <summary>
    ///     Gets or sets whether the defined <see cref="ExecuteConditionAttribute{T}"/>'s for this execution should be ran.
    /// </summary>
    public bool SkipConditions { get; set; } = false;

    /// <summary>
    ///     Gets or sets the separator used to join remaining arguments in a command.
    /// </summary>
    public char RemainderSeparator { get; set; } = ' ';

    /// <summary>
    ///     Gets the default options for command execution.
    /// </summary>
    public static ExecutionOptions Default { get; } = new();
}
