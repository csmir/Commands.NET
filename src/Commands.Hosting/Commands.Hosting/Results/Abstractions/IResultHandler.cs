namespace Commands.Hosting;

/// <summary>
///     An interface that defines a handler for command execution results, allowing for custom handling of success and failure scenarios.
/// </summary>
public interface IResultHandler
{
    /// <summary>
    ///     Handles the result of a command execution, allowing for custom handling of different result types and exceptions.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="exception">The exception that occurred during execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation.</returns>
    public ValueTask<bool> Failure(IContext context, IResult result, Exception exception, IServiceProvider services, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Handles the successful result of a command execution, allowing for custom handling of the result and services used in the execution.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="result">The result of the command execution.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> representing the result of this operation. When the result contains <see langword="true"/>, the handler will stop processing.</returns>
    public ValueTask<bool> Success(IContext context, IResult result, IServiceProvider services, CancellationToken cancellationToken = default);
}
