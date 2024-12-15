﻿using Commands.Reflection;

namespace Commands
{
    /// <summary>
    ///     Represents data about a command, as a <see cref="CommandModule"/> normally would. 
    ///     This context is used for <see langword="static"/> and <see langword="delegate"/> commands.
    /// </summary>
    public class CommandContext<T>(T consumer, CommandInfo command, CommandTree tree, CommandOptions options)
        where T : CallerContext
    {
        /// <summary>
        ///     Gets the consumer of the command currently in scope.
        /// </summary>
        public T Caller { get; } = consumer;

        /// <summary>
        ///     Gets the options for the command currently in scope.
        /// </summary>
        public CommandOptions Options { get; } = options;

        /// <summary>
        ///     Gets the reflection information about the command currently in scope.
        /// </summary>
        public CommandInfo Command { get; } = command;

        /// <summary>
        ///     Gets the command manager responsible for executing the current pipeline.
        /// </summary>
        public CommandTree Tree { get; } = tree;

        /// <summary>
        ///     Sends a response to the consumer of the command.
        /// </summary>
        /// <param name="response">The response to send to the consumer.</param>
        /// <returns>An asynchronous <see cref="Task"/> that can be awaited to wait for the response to send, otherwise dismissed.</returns>
        public Task Send(object response)
            => Caller.Respond(response);
    }
}