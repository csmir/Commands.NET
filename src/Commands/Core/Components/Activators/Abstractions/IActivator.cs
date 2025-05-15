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
    ///     Gets the index of the <see cref="ICallerContext"/> parameter in the target method, which is used to define the context of the command.
    /// </summary>
    /// <remarks>
    ///     Returns -1 if the target method does not have a <see cref="ICallerContext"/> parameter.
    /// </remarks>
    public int ContextIndex { get; }

    /// <summary>
    ///     Invokes the target of this <see cref="IActivator"/> with the provided values.
    /// </summary>
    /// <param name="caller">The caller requesting an instance of the component.</param>
    /// <param name="command">Reflected information of the command that is currently being executed.</param>
    /// <param name="args">The converted arguments to invoke the command with.</param>
    /// <param name="options">The options that determine the execution pattern of this invoker.</param>
    /// <returns>The result of the invocation. This result is <see langword="null"/> if the method signature returns void.</returns>
    public object? Invoke<T>(T caller, Command? command, object?[] args, ExecutionOptions options)
        where T : ICallerContext;
}
