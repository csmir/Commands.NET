namespace Commands;

internal readonly struct CommandInstanceActivator(MethodInfo target) : IActivator
{
    public MethodBase Target
        => target;

    public int ContextIndex
        => -1;

    public object? Invoke<T>(T caller, Command? command, object?[] args, ExecutionOptions options)
        where T : ICallerContext
    {
        var module = command!.Parent?.Activator?.Activate(options);

        if (module != null)
        {
            module.Caller = caller;
            module.Command = command;
        }

        return Target.Invoke(module, args);
    }
}
