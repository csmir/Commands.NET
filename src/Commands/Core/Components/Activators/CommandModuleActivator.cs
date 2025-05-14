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
    {
        var resolver = options.ServiceProvider.GetService(typeof(IDependencyResolver)) as IDependencyResolver 
            ?? new DefaultDependencyResolver(options.ServiceProvider);

        var para = new object?[Dependencies!.Length];

        for (int i = 0; i < Dependencies.Length; i++)
        {
            var dep = Dependencies[i];

            var service = resolver.GetService(dep);

            if (service != null || dep.IsNullable)
                para[i] = service;

            else if (dep.Type == typeof(IServiceProvider))
                para[i] = options.ServiceProvider;

            else if (dep.Type == typeof(IComponentProvider))
                para[i] = options.Provider;

            else if (dep.IsOptional)
                para[i] = Type.Missing;

            else
                throw new InvalidOperationException($"Module {Type.Name} defines unknown service type {dep.Type}.");
        }

        return (CommandModule)_ctor.Invoke(para);
    }
}
