using Commands.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Reflection
{
    /// <summary>
    ///     
    /// </summary>
    public sealed class ConstructorInvoker : IInvoker
    {
        private readonly Type c_serviceType = typeof(IServiceProvider);

        private readonly ConstructorInfo _ctor;

        /// <inheritdoc />
        public MethodBase Target
        {
            get
            {
                return _ctor;
            }
        }

        /// <summary>
        ///     Gets a collection of parameters for the constructor.
        /// </summary>
        public IParameter[] Parameters { get; }

        internal ConstructorInvoker(Type type, BuildOptions options)
        {
            var ctor = GetConstructor(type);

            _ctor = ctor;

            Parameters = ctor.GetParameters(options);
        }

        private static ConstructorInfo GetConstructor(Type type)
        {
            var ctors = type.GetConstructors()
                .OrderByDescending(x => x.GetParameters().Length);

            foreach (var ctor in ctors)
            {
                if (ctor.GetCustomAttributes().Any(attr => attr is SkipAttribute))
                {
                    continue;
                }

                return ctor;
            }

            ThrowHelpers.ThrowInvalidOperation($"{type} is marked as {nameof(ModuleBase)}, but no public constructors are accessible for this type to be created.");

            // this will never be reached.
            return null!;
        }

        /// <inheritdoc />
        public object? Invoke(ConsumerBase consumer, CommandInfo command, object?[] args, CommandOptions options)
        {
            var services = new object?[Parameters.Length];
            for (int i = 0; i < Parameters.Length; i++)
            {
                var parameter = Parameters[i];

                var service = options.Services.GetService(parameter.Type);

                if (service == null)
                {
                    if (parameter.Type == c_serviceType)
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

                    ThrowHelpers.ThrowInvalidOperation($"{Target.Name} defines {parameter.Type} but the {nameof(IServiceProvider)} does not know a service by that type.");
                }

                services[i] = service;
            }

            return _ctor.Invoke(services);
        }
    }
}
