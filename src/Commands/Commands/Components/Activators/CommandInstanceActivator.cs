namespace Commands;

internal readonly struct CommandInstanceActivator(MethodInfo target) : IActivator
{
    public MethodBase Target
        => target;

    public int ContextIndex
        => -1;

    public object? Invoke<TContext>(TContext context, Command? command, object?[] args, ExecutionOptions options)
        where TContext : IContext
    {
        var module = command!.Parent?.Activator?.Activate(options);

        if (module != null)
        {
            module.Context = context;
            module.Command = command;
        }

        return Target.Invoke(module, args);
    }
}
