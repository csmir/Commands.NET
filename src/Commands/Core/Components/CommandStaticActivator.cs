namespace Commands;

/// <summary>
///     An invoker for static commands.
/// </summary>
public sealed class CommandStaticActivator : IActivator
{
    private readonly MethodInfo _method;

    /// <inheritdoc />
    public MethodBase Target
        => _method;

    /// <inheritdoc />
    public bool HasContext { get; }

    internal CommandStaticActivator(MethodInfo target)
    {
        HasContext = target.HasContextProvider();

        _method = target;
    }

    /// <inheritdoc />
    public object? Invoke<T>(T caller, Command? command, object?[] args, CommandOptions options)
        where T : ICallerContext
    {
        if (HasContext)
        {
            var context = new CommandContext<T>(caller, command!, options);

            return Target.Invoke(null, [context, .. args]);
        }

        return Target.Invoke(null, args);
    }
}
