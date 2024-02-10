using Commands.Core;
using Commands.Exceptions;
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
    ///     An invoker for delegate commands.
    /// </summary>
    public sealed class DelegateInvoker : IInvokable
    {
        private readonly object? _instance;

        /// <inheritdoc />
        public MethodInfo Target { get; }

        internal DelegateInvoker(MethodInfo target, object? instance)
        {
            _instance = instance;
            Target = target;
        }

        /// <inheritdoc />
        public async ValueTask<InvokeResult> InvokeAsync(ConsumerBase consumer, CommandInfo command, object?[] args, CommandOptions options)
        {
            var context = new CommandContext(consumer, command, options);

            var result = Target.Invoke(_instance, [context, .. args]);

            switch (result)
            {
                case Task task:
                    {
                        await task;
                        return InvokeResult.FromSuccess(command);
                    }
                case null:
                    {
                        return InvokeResult.FromSuccess(command);
                    }
                default:
                    {
                        // this should never occur, as delegate invocation can only handle Func<Task> and Action.
                        return InvokeResult.FromError(command, InvokeException.ReturnUnresolved());
                    }
            }
        }
    }
}
