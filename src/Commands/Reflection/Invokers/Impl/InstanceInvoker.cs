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
        public object? Invoke<T>(T consumer, CommandInfo command, object?[] args, CommandManager manager, CommandOptions options)
            where T : ConsumerBase
        {
            var module = command.Module!.Invoker.Invoke(consumer, command, args, manager, options) as ModuleBase;

            module!.Consumer = consumer;
            module!.Command = command;
            module!.Manager = manager;

            return Target.Invoke(module, args);
        }
    }
}
