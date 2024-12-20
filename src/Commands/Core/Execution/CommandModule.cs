﻿namespace Commands
{
    /// <summary>
    ///     Represents a <see cref="CommandModule"/> that implements an implementation-friendly accessor to the <see cref="ICallerContext"/>.
    /// </summary>
    /// <typeparam name="T">The implementation of <see cref="ICallerContext"/> known during command pipeline execution.</typeparam>
    public abstract class CommandModule<T> : CommandModule
        where T : ICallerContext
    {
        private T? _consumer;

        /// <summary>
        ///     Gets the consumer for the command currently in scope.
        /// </summary>
        /// <remarks>
        ///     Throws if the <see cref="ICallerContext"/> provided in this scope does not match <typeparamref name="T"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="T"/> does not match with the provided</exception>
        public new T Caller
        {
            get
            {
                if (_consumer == null)
                {
                    if (base.Caller is T t)
                    {
                        _consumer = t;
                    }
                    else
                        throw new InvalidOperationException($"{base.Caller.GetType()} cannot be cast to {typeof(T)}.");
                }
                return _consumer;
            }
        }
    }

    /// <summary>
    ///     The base type needed to write commands with Commands.NET. This type can be derived freely, in order to extend and implement command functionality. 
    ///     Modules do not have state, they are instantiated and populated before a command runs and immediately disposed when it finishes.
    /// </summary>
    /// <remarks>
    ///      All derived types must be passed to the <see cref="IComponentTree"/> to be discoverable and automatically registered during creation.
    /// </remarks>
    public abstract class CommandModule
    {
        /// <summary>
        ///     Gets the consumer for the command currently in scope.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
        public ICallerContext Caller { get; internal set; }

        /// <summary>
        ///     Gets the reflection information about the command currently in scope.
        /// </summary>
        public CommandInfo Command { get; internal set; }

        /// <summary>
        ///     Gets the command manager that is responsible for the current command pipeline.
        /// </summary>
        public IComponentTree Tree { get; internal set; }
#pragma warning restore CS8618

        /// <summary>
        ///     Sends a response to the consumer.
        /// </summary>
        /// <param name="response">The response to send to the consumer.</param>
        /// <returns>An asynchronous <see cref="Task"/> containing the state of the response. This call does not need to be awaited, running async if not.</returns>
        public Task Respond(object response)
        {
            return Caller.Respond(response);
        }
    }
}
