namespace Commands;

internal readonly struct CommandGroupActivator : IActivator
{
    private readonly ConstructorInfo _ctor;
    private readonly CommandGroupService[] _services;

    public MethodBase Target
        => _ctor;

    public bool HasContext
        => false;

    public CommandGroupActivator(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        Type type)
    {
        _ctor = type.GetAvailableConstructor();

        var parameters = _ctor.GetParameters();

        _services = new CommandGroupService[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
            _services[i] = new CommandGroupService(parameters[i]);
    }

    /// <inheritdoc />
    public object? Invoke<T>(T caller, Command? command, object?[] args, CommandOptions options)
        where T : ICallerContext
    {
        var obj = options.Services.GetService(Target.DeclaringType!);

        if (obj == null)
        {
            var services = new object?[_services.Length];
            for (int i = 0; i < _services.Length; i++)
            {
                var parameter = _services[i];

                var service = options.Services.GetService(parameter.Type);

                if (service != null || parameter.IsNullable)
                    services[i] = service;

                else if (parameter.Type == typeof(IServiceProvider))
                    services[i] = options.Services;

                else if (parameter.IsOptional)
                    services[i] = Type.Missing;

                else
                    throw new InvalidOperationException($"Constructor {command?.Parent?.Name ?? Target.Name} defines unknown service {parameter.Type}.");
            }

            return _ctor.Invoke(services);
        }
        return obj;
    }
}
