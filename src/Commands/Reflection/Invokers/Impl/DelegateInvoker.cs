using System.Reflection;

namespace Commands.Reflection
{
    /// <summary>
    ///     An invoker for delegate commands.
    /// </summary>
    public sealed class DelegateInvoker : IInvoker
    {
        private readonly object? _instance;
        private readonly bool _withContext;

        /// <inheritdoc />
        public MethodBase Target { get; }

        internal DelegateInvoker(MethodInfo target, object? instance, bool withContext)
        {
            _withContext = withContext;
            _instance = instance;
            Target = target;
        }

        /// <inheritdoc />
        public object? Invoke<T>(T consumer, CommandInfo command, object?[] args, CommandManager manager, CommandOptions options)
            where T : ConsumerBase
        {
            if (_withContext)
            {
                var context = new CommandContext<T>(consumer, command, manager, options);

                return Target.Invoke(_instance, [context, .. args]);
            }

            return Target.Invoke(_instance, args);
        }
    }
}
