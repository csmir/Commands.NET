using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Commands
{
    /// <summary>
    ///     An invoker that invokes a constructor.
    /// </summary>
    public sealed class ModuleActivator : IActivator
    {
        private readonly ConstructorInfo _ctor;
        /// <summary>
        ///     Gets a collection of parameters for the constructor.
        /// </summary>
        public IParameter[] Parameters { get; }

        /// <inheritdoc />
        public MethodBase Target
            => _ctor;

        internal ModuleActivator(
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
            Type type)
        {
            var ctor = type.GetInvokableConstructor();

            _ctor = ctor;

            var parameters = ctor.GetParameters();

            Parameters = new IParameter[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
                Parameters[i] = new ServiceInfo(parameters[i]);
        }

        /// <inheritdoc />
        public object? Invoke<T>(T caller, CommandInfo? command, object?[] args, IComponentTree? tree, CommandOptions options)
            where T : ICallerContext
        {
            var services = new object?[Parameters.Length];
            for (int i = 0; i < Parameters.Length; i++)
            {
                var parameter = Parameters[i];

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

        /// <inheritdoc />
#if NET8_0_OR_GREATER
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        public Type? GetReturnType()
            => default;
    }
}
