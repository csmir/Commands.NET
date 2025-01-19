namespace Commands;

/// <summary>
///     An invoker for delegate commands.
/// </summary>
public sealed class CommandDelegateActivator : IActivator
{
    private readonly object? _instance;
    private readonly MethodInfo _method;

    /// <inheritdoc />
    public MethodBase Target
        => _method;

    /// <inheritdoc />
    public bool HasContext { get; }

    internal CommandDelegateActivator(Delegate target)
    {
        HasContext = target.Method.HasContextProvider();

        _instance = target.Target;
        _method = target.Method;
    }

    /// <inheritdoc />
    public object? Invoke<T>(T caller, Command? command, object?[] args, CommandOptions options)
        where T : ICallerContext
    {
        if (HasContext)
        {
            var context = new CommandContext<T>(caller, command!, options);

            return Target.Invoke(_instance, [context, .. args]);
        }

        return Target.Invoke(_instance, args);
    }
}
