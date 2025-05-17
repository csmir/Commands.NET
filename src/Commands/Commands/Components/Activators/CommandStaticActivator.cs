namespace Commands;

internal readonly struct CommandStaticActivator : IActivator
{
    private readonly DependencyParameter[] _dependencies;
    private readonly object? _state;

    public MethodBase Target { get; }

    public int ContextIndex { get; }

    public CommandStaticActivator(MethodInfo target, object? state = null)
    {
        Assert.NotNull(target, nameof(target));

        Target = target;
        _state = state;

        ContextIndex = -1;

        var param = target.GetParameters();

        for (var i = 0; i < param.Length; i++)
        {
            if (typeof(IContext).IsAssignableFrom(param[i].ParameterType))
                ContextIndex = i;
        }

        _dependencies = new DependencyParameter[ContextIndex == -1 ? 0 : ContextIndex];

        if (ContextIndex > 0)
        {
            for (var i = 0; i < ContextIndex; i++)
            {
                var dep = param[i];
                _dependencies[i] = new DependencyParameter(dep);
            }
        }
    }

    public object? Invoke<TContext>(TContext context, Command? command, object?[] args, ExecutionOptions options)
        where TContext : IContext
    {
        if (ContextIndex != -1)
            return Target.Invoke(_state, [.. _dependencies.Resolve(Target, options), context, .. args]);

        return Target.Invoke(_state, args);
    }
}
