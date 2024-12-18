using System.Reflection;

namespace Commands.Components
{
    /// <summary>
    ///     An invoker for instanced commands.
    /// </summary>
    public sealed class InstanceActivator : IActivator
    {
        private readonly MethodInfo _method;

        /// <inheritdoc />
        public MethodBase Target
            => _method;

        internal InstanceActivator(MethodInfo target)
        {
            _method = target;
        }

        /// <inheritdoc />
        public object? Invoke<T>(T consumer, CommandInfo command, object?[] args, ComponentTree manager, CommandOptions options)
            where T : CallerContext
        {
            var module = command.Parent?.Invoker?.Invoke(consumer, command, args, manager, options) as CommandModule;

            if (module != null)
            {
                module.Caller = consumer;
                module.Command = command;
                module.Tree = manager;
            }

            return Target.Invoke(module, args);
        }

        /// <inheritdoc />
        public Type? GetReturnType()
        {
            return _method.ReturnType;
        }
    }
}
