namespace Commands;

internal readonly struct CommandStaticActivator : IActivator
{
    private readonly int _contextIndex;
    private readonly DependencyParameter[] _dependencies;
    private readonly object? _state;

    public MethodBase Target { get; }

    public int SignatureLength { get; }

    public CommandStaticActivator(MethodInfo target, object? state = null)
    {
        Target = target;
        _state = state;

        _contextIndex = -1;
        _dependencies = [];

        var parameters = target.GetParameters();

        for (var i = 0; i < parameters.Length; i++)
        {
            if (typeof(IContext).IsAssignableFrom(parameters[i].ParameterType))
                _contextIndex = i;

            if (parameters[i].GetCustomAttributes().OfType<DependencyAttribute>().Any())
                Utilities.CopyTo(ref _dependencies, new DependencyParameter(parameters[i]));
        }

        SignatureLength = parameters.Length;
    }

    public object? Invoke<TContext>(TContext context, Command? command, object?[] args, ExecutionOptions options)
        where TContext : IContext
    {
        Utilities.ResolveDependencies(ref args, _dependencies, Target, options);

        if (_contextIndex != -1)
            args[_contextIndex] = context;

        return Target.Invoke(_state, args);
    }

    public ICommandParameter[] GetParameters(ComponentOptions options)
        => Utilities.GetCommandParameters(Target.GetParameters(), _dependencies, _contextIndex, options);
}
