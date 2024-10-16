using System.Reflection;

namespace Commands.Reflection
{
    /// <summary>
    ///     An invoker for static commands.
    /// </summary>
    public sealed class StaticInvoker : IInvoker
    {
        private readonly bool _withContext;
        /// <inheritdoc />
        public MethodBase Target { get; }

        internal StaticInvoker(MethodInfo target, bool withContext)
        {
            _withContext = withContext;
            Target = target;
        }

        /// <inheritdoc />
        public object? Invoke<T>(T consumer, CommandInfo command, object?[] args, CommandManager manager, CommandOptions options)
            where T : ConsumerBase
        {
            if (_withContext)
            {
                var context = new CommandContext<T>(consumer, command, manager, options);

                return Target.Invoke(null, [context, .. args]);
            }

            return Target.Invoke(null, args);
        }
    }
}
