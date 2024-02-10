using Commands.Core;
using Commands.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Reflection
{
    /// <summary>
    ///     An invoker for instanced commands.
    /// </summary>
    public sealed class InstanceInvoker : IInvokable
    {
        /// <inheritdoc />
        public MethodInfo Target { get; }

        internal InstanceInvoker(MethodInfo target)
        {
            Target = target;
        }

        /// <inheritdoc />
        public async ValueTask<InvokeResult> InvokeAsync(ConsumerBase consumer, CommandInfo command, object?[] args, CommandOptions options)
        {
            var targetInstance = options.Scope!.ServiceProvider.GetService(command.Module!.Type); // never null for ModuleInvoker.

            var module = targetInstance != null // never null in casting logic.
                ? (targetInstance as ModuleBase)!
                : (ActivatorUtilities.CreateInstance(options.Scope.ServiceProvider, command.Module.Type) as ModuleBase)!;

            module.Consumer = consumer;
            module.Command = command;
            module.Logger = options.Logger!;

            var value = Target.Invoke(module, args);

            var result = await module.ResolveReturnAsync(value);

            return result;
        }
    }
}
