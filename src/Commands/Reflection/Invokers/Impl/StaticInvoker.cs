using Commands.Core;
using Commands.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Reflection
{
    /// <summary>
    ///     An invoker for static commands.
    /// </summary>
    public sealed class StaticInvoker : IInvokable
    {
        /// <inheritdoc />
        public MethodInfo Target { get; }

        internal StaticInvoker(MethodInfo target)
        {
            Target = target;
        }

        /// <inheritdoc />
        public async ValueTask<InvokeResult> InvokeAsync(ConsumerBase consumer, CommandInfo command, object?[] args, CommandOptions options)
        {
            var context = new CommandContext(consumer, command, options);

            var result = Target.Invoke(null, [context, .. args]);

            switch (result)
            {
                case Task task:
                    {
                        await task;
                        return InvokeResult.FromSuccess(command);
                    }
                case ValueTask valueTask:
                    {
                        await valueTask;
                        return InvokeResult.FromSuccess(command);
                    }
                case null:
                    {
                        return InvokeResult.FromSuccess(command);
                    }
                default:
                    {
                        return InvokeResult.FromError(command, InvokeException.ReturnUnresolved());
                    }
            }
        }
    }
}
