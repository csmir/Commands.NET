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
        var module = command!.Parent?.Activator?.Activate(options.ServiceProvider);

        if (module != null)
        {
            module.Caller = caller;
            module.Command = command;
        }

        return Target.Invoke(module, args);
    }
}
