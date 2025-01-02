using System.Reflection;

namespace Commands;

/// <summary>
///     An invoker for delegate commands.
/// </summary>
public sealed class DelegateActivator : IActivator
{
    private readonly object? _instance;
    private readonly bool _withContext;
    private readonly MethodInfo _method;

    /// <inheritdoc />
    public MethodBase Target
        => _method;

    internal DelegateActivator(MethodInfo target, object? instance, bool withContext)
    {
        _withContext = withContext;
        _instance = instance;
        _method = target;
    }

    /// <inheritdoc />
    public object? Invoke<T>(T caller, CommandInfo? command, object?[] args, IComponentTree? tree, CommandOptions options)
        where T : ICallerContext
    {
        if (_withContext)
        {
            var context = new CommandContext<T>(caller, command!, tree!, options);

            return Target.Invoke(_instance, [context, .. args]);
        }

        return Target.Invoke(_instance, args);
    }
}
