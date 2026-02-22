namespace Commands;

internal readonly struct CommandModuleActivator : IDependencyActivator<CommandModule>
{
    private readonly ConstructorInfo _ctor;
    private readonly DependencyParameter[] _dependencies;

#if NET6_0_OR_GREATER
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
    public Type Type { get; }

    public CommandModuleActivator(
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type type)
    {
        _ctor = type.GetAvailableConstructor();

        var parameters = _ctor.GetParameters();

        // We know that all parameters on a module are dependencies so we can do this without checking for attributes.
        _dependencies = new DependencyParameter[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
            _dependencies[i] = new DependencyParameter(parameters[i]);

        Type = type;
    }

    public CommandModule Activate(ExecutionOptions options)
    {
        var args = Array.Empty<object?>();

        Utilities.ResolveDependencies(ref args, _dependencies, _ctor, options);

        return (CommandModule)_ctor.Invoke(args);
    }
}
