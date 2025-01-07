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
    ///     Invokes the target of this <see cref="IActivator"/> with the provided values.
    /// </summary>
    /// <param name="caller">The caller requesting an instance of the component.</param>
    /// <param name="command">Reflected information of the command that is currently being executed.</param>
    /// <param name="args">The converted arguments to invoke the command with.</param>
    /// <param name="options">The options that determine the execution pattern of this invoker.</param>
    /// <returns>The result of the invocation. This result is <see langword="null"/> if the method signature returns void.</returns>
    public object? Invoke<T>(T caller, Command? command, object?[] args, CommandOptions options)
        where T : ICallerContext;
}
