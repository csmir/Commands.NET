namespace Commands;

internal readonly struct CommandModuleActivator : IDependencyActivator<CommandModule>
{
    private readonly ConstructorInfo _ctor;

#if NET8_0_OR_GREATER
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
    public Type Type { get; }

    public DependencyParameter[] Dependencies { get; }

    public CommandModuleActivator(
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type type)
    {
        _ctor = type.GetAvailableConstructor();

        var parameters = _ctor.GetParameters();

        Dependencies = new DependencyParameter[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
            Dependencies[i] = new DependencyParameter(parameters[i]);

        Type = type;
    }

    public CommandModule Activate(ExecutionOptions options)
        => (CommandModule)_ctor.Invoke(Utilities.ResolveDependencies(Dependencies, _ctor, options));
}
