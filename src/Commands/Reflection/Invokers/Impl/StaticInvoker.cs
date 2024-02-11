using Commands.Exceptions;
using System.Reflection;

namespace Commands.Reflection
{
    /// <summary>
    ///     An invoker for static commands.
    /// </summary>
    public sealed class StaticInvoker : IInvoker
    {
        /// <inheritdoc />
        public MethodBase Target { get; }

        internal StaticInvoker(MethodInfo target)
        {
            Target = target;
        }

        /// <inheritdoc />
        public object? Invoke(ConsumerBase consumer, CommandInfo command, object?[] args, CommandOptions options)
        {
            var context = new CommandContext(consumer, command, options);

            return Target.Invoke(null, [context, .. args]);
        }
    }
}
