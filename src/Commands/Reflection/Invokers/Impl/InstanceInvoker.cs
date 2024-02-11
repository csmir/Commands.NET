using System.Reflection;

namespace Commands.Reflection
{
    /// <summary>
    ///     An invoker for instanced commands.
    /// </summary>
    public sealed class InstanceInvoker : IInvoker
    {
        /// <inheritdoc />
        public MethodBase Target { get; }

        internal InstanceInvoker(MethodInfo target)
        {
            Target = target;
        }

        /// <inheritdoc />
        public object? Invoke(ConsumerBase consumer, CommandInfo command, object?[] args, CommandOptions options)
        {
            var module = command.Module!.Invoker.Invoke(consumer, command, args, options) as ModuleBase;

            module!.Consumer = consumer;
            module!.Command = command;

            return Target.Invoke(module, args);
        }
    }
}
