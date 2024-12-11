using System.Reflection;

namespace Commands.Reflection
{
    /// <summary>
    ///     An invoker for a command.
    /// </summary>
    public interface IInvoker
    {
        /// <summary>
        ///     Gets the target to invoke.
        /// </summary>
        public MethodBase Target { get; }

        /// <summary>
        ///     Invokes the target of this <see cref="IInvoker"/> with the provided arguments.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="command">Reflected information of the command.</param>
        /// <param name="args">The converted arguments to invoke the command with.</param>
        /// <param name="manager">The command manager responsible for executing the current pipeline.</param>
        /// <param name="options">The options that determine the execution pattern of this invoker.</param>
        /// <returns>The result of the invocation. This result is <see langword="null"/> if the method signature returns void.</returns>
        public object? Invoke<T>(T consumer, CommandInfo command, object?[] args, CommandManager manager, CommandOptions options)
            where T : ConsumerBase;

        /// <summary>
        ///     Gets the return type of the target, if it is a method. If it is a constructor, it will return null.
        /// </summary>
        /// <returns>A type representing the returned value of the invoked target.</returns>
        public Type? GetReturnType();
    }
}
