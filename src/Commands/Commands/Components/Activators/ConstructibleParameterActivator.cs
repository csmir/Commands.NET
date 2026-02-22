namespace Commands;

internal readonly struct ConstructibleParameterActivator
    : IActivator
{
    private readonly ConstructorInfo _ctor;
    private readonly DependencyParameter[] _dependencies;

    public MethodBase Target
        => _ctor;

    public int SignatureLength { get; }

    public ConstructibleParameterActivator(
#if NET6_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        Type type)
    {
        _ctor = type.GetAvailableConstructor();
        _dependencies = [];

        var parameters = _ctor.GetParameters();

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
        Utilities.ResolveDependencies(ref args, _dependencies, _ctor, options);

        return _ctor.Invoke(args);
    }

    public ICommandParameter[] GetParameters(ComponentOptions options)
        => Utilities.GetCommandParameters(_ctor.GetParameters(), _dependencies, -1, options);
}
