namespace Commands;

internal readonly struct CommandInstanceActivator(MethodInfo target) : IActivator
{
    /// <inheritdoc />
    public MethodBase Target
        => target;

    /// <inheritdoc />
    public bool HasContext
        => false;

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
