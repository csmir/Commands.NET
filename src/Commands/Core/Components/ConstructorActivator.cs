using System.Reflection;

namespace Commands
{
    /// <summary>
    ///     An invoker that invokes a constructor.
    /// </summary>
    public sealed class ConstructorActivator : IActivator
    {
        private readonly ConstructorInfo _ctor;

        /// <summary>
        ///     Gets a collection of parameters for the constructor.
        /// </summary>
        public IParameter[] Parameters { get; }

        /// <inheritdoc />
        public MethodBase Target
            => _ctor;

        internal ConstructorActivator(Type type)
        {
            var ctor = GetConstructor(type);

            _ctor = ctor;

            var parameters = ctor.GetParameters();

            Parameters = new IParameter[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
                Parameters[i] = new ServiceInfo(parameters[i]);
        }

        private static ConstructorInfo GetConstructor(Type type)
        {
            var ctors = type.GetConstructors()
                .OrderByDescending(x => x.GetParameters().Length);

            foreach (var ctor in ctors)
            {
                if (ctor.GetCustomAttributes().Any(attr => attr is IgnoreAttribute))
                    continue;

                return ctor;
            }

            throw new InvalidOperationException($"{type} has no public constructors that are accessible for this type to be constructed.");
        }

        /// <inheritdoc />
        public object? Invoke<T>(T caller, CommandInfo? command, object?[] args, IComponentTree? tree, CommandOptions options)
            where T : ICallerContext
        {
            // When building a complex object, one or more arguments have to be passed to the constructor in any case.
            // This way, we can use the same invoker for complex constructors and module constructors.
            if (args.Length != 0)
                return _ctor.Invoke(args);

            var services = new object?[Parameters.Length];
            for (int i = 0; i < Parameters.Length; i++)
            {
                var parameter = Parameters[i];

                var service = options.Services.GetService(parameter.Type);

                if (service == null)
                {
                    if (parameter.Type == typeof(IServiceProvider))
                    {
                        services[i] = options.Services;
                        continue;
                    }

                    if (parameter.IsNullable)
                    {
                        services[i] = null;
                        continue;
                    }

                    if (parameter.IsOptional)
                    {
                        services[i] = Type.Missing;
                        continue;
                    }

                    throw new InvalidOperationException($"Constructor {command?.Parent?.Name ?? Target.Name} defines {parameter.Type} but {c_serviceType.Name} does not know a service by that type.");
                }

                services[i] = service;
            }

            return _ctor.Invoke(services);
        }

        /// <inheritdoc />
        public Type? GetReturnType()
            => default;
    }
}
