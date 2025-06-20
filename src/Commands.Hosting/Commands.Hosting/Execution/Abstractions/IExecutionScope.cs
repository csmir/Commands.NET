namespace Commands.Hosting;

/// <summary>
///     Represents the execution of a command.
/// </summary>
/// <remarks>
///     The context is scoped to the command execution. A scope is created when the factory begins executing a command, and is disposed of when the command finishes executing.
/// </remarks>
public interface IExecutionScope : IDisposable
{
    /// <summary>
    ///     Gets the <see cref="IContext"/> resembling the metadata and response mechanism of the command being executed in the scope.
    /// </summary>
    public IContext Context { get; set; }

    /// <summary>
    ///     Gets the <see cref="IServiceScope"/> that contains the services used to execute the command in this scope. This scope is created when the command execution starts and disposed of when the command execution ends.
    /// </summary>
    public IServiceScope Scope { get; set; }
}
