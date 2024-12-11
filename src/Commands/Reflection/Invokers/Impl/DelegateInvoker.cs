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
        private readonly MethodInfo _method;

        /// <inheritdoc />
        public MethodBase Target
            => _method;

        internal DelegateInvoker(MethodInfo target, object? instance, bool withContext)
        {
            _withContext = withContext;
            _instance = instance;
            _method = target;
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

        /// <inheritdoc />
        public Type? GetReturnType()
        {
            return _method.ReturnType;
        }
    }
}
