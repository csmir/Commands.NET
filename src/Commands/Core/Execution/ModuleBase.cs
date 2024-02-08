using Commands.Helpers;
using Commands.Reflection;
using Microsoft.Extensions.Logging;

namespace Commands.Core
{
    /// <summary>
    ///     Represents a <see cref="ModuleBase"/> that implements an implementation-friendly accessor to the <see cref="ConsumerBase"/>.
    /// </summary>
    /// <typeparam name="T">The implementation of <see cref="ConsumerBase"/> known during command pipeline execution.</typeparam>
    public abstract class ModuleBase<T> : ModuleBase
        where T : ConsumerBase
    {
        private T? _consumer;

        /// <summary>
        ///     Gets the consumer for the command currently in scope.
        /// </summary>
        /// <remarks>
        ///     Throws if the <see cref="ConsumerBase"/> provided in this scope does not match <typeparamref name="T"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="T"/> does not match with the provided</exception>
        public new T Consumer
        {
            get
            {
                if (_consumer == null)
                {
                    if (base.Consumer is T t)
                    {
                        _consumer = t;
                    }
                    else
                    {
                        ThrowHelpers.ThrowInvalidOperation($"{Consumer.GetType()} cannot be cast to {typeof(T)}.");
                    }
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
    ///      All derived types must be known in <see cref="BuildOptions.Assemblies"/> to be discoverable and automatically registered during the creation of a <see cref="CommandManager"/>.
    /// </remarks>
    public abstract class ModuleBase
    {
        /// <summary>
        ///     Gets the consumer for the command currently in scope.
        /// </summary>
        /// <remarks>
        ///     Can be null if not provided by <see cref="CommandManager.TryExecuteAsync{T}(T, object[], CommandOptions?)"/>.
        /// </remarks>
        public ConsumerBase? Consumer { get; internal set; }

        /// <summary>
        ///     Gets the reflection information about the command currently in scope.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
        public CommandInfo Command { get; internal set; } // never null as set internally.

        /// <summary>
        ///     Gets the logger that logs information for the command currently in scope.
        /// </summary>
        public ILogger Logger { get; internal set; } // never null as set internally.
#pragma warning restore CS8618

        /// <summary>
        ///     Represents an overridable operation that is responsible for resolving unknown invocation results.
        /// </summary>
        /// <param name="value">The invocation result of which no base handler exists.</param>
        /// <returns>The awaitable result of this asynchronous operation.</returns>
        public virtual async ValueTask UnknownReturnAsync(object value)
        {
            await Task.CompletedTask;
        }

        internal async Task<InvokeResult> ResolveReturnAsync(object? value)
        {
            switch (value)
            {
                case Task task:
                    await task;
                    break;
                case ValueTask vTask:
                    await vTask;
                    break;
                case null:
                    break;
                default:
                    await UnknownReturnAsync(value);
                    break;
            }

            return InvokeResult.FromSuccess(Command);
        }
    }
}
