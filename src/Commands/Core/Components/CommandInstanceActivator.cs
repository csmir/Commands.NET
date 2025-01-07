namespace Commands;

/// <summary>
///     An invoker for instanced commands.
/// </summary>
public sealed class CommandInstanceActivator : IActivator
{
    private readonly MethodInfo _method;

    /// <inheritdoc />
    public MethodBase Target
        => _method;

    internal CommandInstanceActivator(MethodInfo target)
    {
        _method = target;
    }

    /// <inheritdoc />
    public object? Invoke<T>(T caller, Command? command, object?[] args, CommandOptions options)
        where T : ICallerContext
    {
        var module = command!.Parent?.Activator?.Invoke(caller, command, args, options) as CommandModule;

        if (module != null)
        {
            module.Caller = caller;
            module.Command = command;
            module.Manager = options.Manager;
        }

        return Target.Invoke(module, args);
    }
}
