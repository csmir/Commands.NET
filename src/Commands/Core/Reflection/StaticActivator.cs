using System.Reflection;

namespace Commands
{
    /// <summary>
    ///     An invoker for static commands.
    /// </summary>
    public sealed class StaticActivator : IActivator
    {
        private readonly bool _withContext;
        private readonly MethodInfo _method;

        /// <inheritdoc />
        public MethodBase Target
            => _method;

        internal StaticActivator(MethodInfo target, bool withContext)
        {
            _withContext = withContext;
            _method = target;
        }

        /// <inheritdoc />
        public object? Invoke<T>(T caller, CommandInfo? command, object?[] args, ComponentTree? tree, CommandOptions options)
            where T : ICallerContext
        {
            if (_withContext)
            {
                var context = new CommandContext<T>(caller, command!, tree!, options);

                return Target.Invoke(null, [context, .. args]);
            }

            return Target.Invoke(null, args);
        }

        /// <inheritdoc />
        public Type? GetReturnType()
            => _method.ReturnType;
    }
}
