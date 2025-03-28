﻿namespace Commands;

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

    public CommandModule Activate(IServiceProvider services)
    {
        var obj = services.GetService(Type!);

        if (obj == null)
        {
            var param = new object?[Dependencies!.Length];

            for (int i = 0; i < Dependencies.Length; i++)
            {
                var parameter = Dependencies[i];

                var service = services.GetService(parameter.Type);

                if (service != null || parameter.IsNullable)
                    param[i] = service;

                else if (parameter.Type == typeof(IServiceProvider))
                    param[i] = services;

                else if (parameter.IsOptional)
                    param[i] = Type.Missing;

                else
                    throw new InvalidOperationException($"Module {Type!.Name} defines unknown service {parameter.Type}.");
            }

            return (CommandModule)_ctor.Invoke(param);
        }

        return (CommandModule)obj;
    }
}
