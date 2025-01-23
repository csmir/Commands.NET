namespace Commands;

internal readonly struct CommandStaticActivator(MethodInfo target, object? state = null) : IActivator
{
    public MethodBase Target
        => target;

    public bool HasContext { get; } = target.HasContextProvider();

    public object? Invoke<T>(T caller, Command? command, object?[] args, CommandOptions options)
        where T : ICallerContext
    {
        if (HasContext)
        {
            var context = new CommandContext<T>(caller, command!, options);

            return Target.Invoke(state, [context, .. args]);
        }

        return Target.Invoke(state, args);
    }
}
