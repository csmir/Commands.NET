namespace Commands;

/// <summary>
///     An activator which initializes a target using its preferred initialization target.
/// </summary>
public interface IActivator
{
    /// <summary>
    ///     Gets the target to invoke during activation.
    /// </summary>
    public MethodBase Target { get; }

    /// <summary>
    ///     Gets the total length of parameters expected by the target of this activator, including the parameters that are implicitly passed by the command execution pipeline, such as the context or the command information.
    /// </summary>
    public int SignatureLength { get; }

    /// <summary>
    ///     Invokes the target of this <see cref="IActivator"/> with the provided values.
    /// </summary>
    /// <param name="context">The context requesting an instance of the component.</param>
    /// <param name="command">Reflected information of the command that is currently being executed.</param>
    /// <param name="args">The converted arguments to invoke the command with.</param>
    /// <param name="options">The options that determine the execution pattern of this invoker.</param>
    /// <returns>The result of the invocation. This result is <see langword="null"/> if the method signature returns void.</returns>
    /// <exception cref="ComponentFormatException">Thrown when the service provider could not resolve the service signature, being a set of services defined on the member or module.</exception>
    public object? Invoke<TContext>(TContext context, Command? command, object?[] args, ExecutionOptions options)
        where TContext : IContext;

    /// <summary>
    ///     Gets a parameter collection that contains the parameters the activator expects to be passed to the target.
    /// </summary>
    /// <param name="options">The options that determine the execution pattern of this activator.</param>
    /// <returns>An array containing the <see cref="ICommandParameter"/> implementations for a command that will run based on this activator.</returns>
    public ICommandParameter[] GetParameters(ComponentOptions options);
}
