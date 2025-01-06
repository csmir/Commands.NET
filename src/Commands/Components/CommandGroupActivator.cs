using System.Reflection;

namespace Commands;

/// <summary>
///     An invoker that invokes a command group constructor.
/// </summary>
public sealed class CommandGroupActivator : IActivator
{
    private readonly ConstructorInfo _ctor;

    /// <summary>
    ///     Gets a collection of services for the group.
    /// </summary>
    public CommandGroupService[] Services { get; }

    /// <inheritdoc />
    public MethodBase Target
        => _ctor;

    internal CommandGroupActivator(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        Type type)
    {
        var ctors = type.GetAvailableConstructors();

        _ctor = ctors.First();

        var parameters = _ctor.GetParameters();

        Services = new CommandGroupService[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
            Services[i] = new CommandGroupService(parameters[i]);
    }

    /// <inheritdoc />
    public object? Invoke<T>(T caller, Command? command, object?[] args, CommandOptions options)
        where T : ICallerContext
    {
        var services = new object?[Services.Length];
        for (int i = 0; i < Services.Length; i++)
        {
            var parameter = Services[i];

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
}
