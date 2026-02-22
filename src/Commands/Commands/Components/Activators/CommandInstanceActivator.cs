namespace Commands;

internal readonly struct CommandInstanceActivator
    : IActivator
{
    private readonly DependencyParameter[] _dependencies;

    public MethodBase Target { get; }

    public int SignatureLength { get; }

    public CommandInstanceActivator(MethodInfo target)
    {
        Target = target;

        _dependencies = [];

        var parameters = target.GetParameters();

        foreach (var parameter in parameters)
        {
            if (parameter.GetCustomAttributes().OfType<DependencyAttribute>().Any())
                Utilities.CopyTo(ref _dependencies, new DependencyParameter(parameter));
        }

        SignatureLength = parameters.Length;
    }

    public object? Invoke<TContext>(TContext context, Command? command, object?[] args, ExecutionOptions options)
        where TContext : IContext
    {
        var module = command!.Parent?.Activator?.Activate(options);

        if (module != null)
        {
            module.Context = context;
            module.Command = command;
        }

        Utilities.ResolveDependencies(ref args, _dependencies, Target, options);

        return Target.Invoke(module, args);
    }

    public ICommandParameter[] GetParameters(ComponentOptions options)
        => Utilities.GetCommandParameters(Target.GetParameters(), _dependencies, -1, options);
}
