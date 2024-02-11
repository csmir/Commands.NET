using Commands.Exceptions;
using System.Reflection;

namespace Commands.Reflection
{
    /// <summary>
    ///     An invoker for delegate commands.
    /// </summary>
    public sealed class DelegateInvoker : IInvoker
    {
        private readonly object? _instance;

        /// <inheritdoc />
        public MethodBase Target { get; }

        internal DelegateInvoker(MethodInfo target, object? instance)
        {
            _instance = instance;
            Target = target;
        }

        /// <inheritdoc />
        public object? Invoke(ConsumerBase consumer, CommandInfo command, object?[] args, CommandOptions options)
        {
            var context = new CommandContext(consumer, command, options);

            return Target.Invoke(_instance, [context, .. args]);
        }
    }
}
