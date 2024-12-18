using System.Reflection;

namespace Commands
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
        public object? Invoke<T>(T caller, CommandInfo? command, object?[] args, ComponentTree? tree, CommandOptions options)
            where T : CallerContext
        {
            var module = command!.Parent?.Activator?.Invoke(caller, command, args, tree, options) as CommandModule;

            if (module != null)
            {
                module.Caller = caller;
                module.Command = command;
                module.Tree = tree!;
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
