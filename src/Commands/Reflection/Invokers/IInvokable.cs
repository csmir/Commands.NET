﻿using Commands.Core;
using System.Reflection;

namespace Commands.Reflection
{
    /// <summary>
    ///     An invoker for a command.
    /// </summary>
    public interface IInvokable
    {
        /// <summary>
        ///     The target to invoke.
        /// </summary>
        public MethodInfo Target { get; }

        /// <summary>
        ///     Invokes the target of this <see cref="IInvokable"/> with the provided arguments.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="command">Reflected information of the command.</param>
        /// <param name="args">The converted arguments to invoke the command with.</param>
        /// <param name="options">The options that determine the execution pattern of this invoker.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the invocation.</returns>
        public ValueTask<InvokeResult> InvokeAsync(ConsumerBase consumer, CommandInfo command, object?[] args, CommandOptions options);
    }
}
