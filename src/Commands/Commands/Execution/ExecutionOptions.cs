﻿using Commands.Conditions;
using System.ComponentModel;

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
    ///     Gets or sets whether command execution should be handled asynchronously or not.
    /// </summary>
    /// <remarks>
    ///     This behavior drastically changes execution flow, and as such, there are a few things to consider when using it:
    ///     <list type="bullet">
    ///         <item>
    ///             The end-user must provide an implementation of <see cref="ResultHandler"/> to the <see cref="ComponentTree"/> that is used to execute the command, in order to handle the result of the command.
    ///         </item>
    ///         <item>
    ///             Objects, specifically those scoped to more than a single command must be made thread-safe, meaning they must be able to handle multiple requests at once.
    ///         </item>
    ///     </list>
    ///     When considering thread-safety, it is important to know what this actually means.
    /// </remarks>
    public bool ExecuteAsynchronously { get; set; } = false;

    /// <summary>
    ///     Gets the default options for command execution.
    /// </summary>
    public static ExecutionOptions Default { get; } = new();

    /// <inheritdoc cref="IServiceProvider" />
    /// <remarks>
    ///     This class implements the <see cref="IServiceProvider"/> interface and returns <see langword="null"/> for all service requests.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class EmptyServiceProvider : IServiceProvider
    {
        internal static EmptyServiceProvider Instance { get; } = new();

        /// <inheritdoc />
        public object? GetService(Type serviceType)
            => null;
    }
}
