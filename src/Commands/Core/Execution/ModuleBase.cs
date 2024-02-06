using Commands.Reflection;

namespace Commands.Core
{
    /// <summary>
    ///     Represents a <see cref="ModuleBase"/> that implements an implementation-friendly accessor to the <see cref="ICommandContext"/>.
    /// </summary>
    /// <typeparam name="T">The implementation of <see cref="ICommandContext"/> known during command pipeline execution.</typeparam>
    public abstract class ModuleBase<T> : ModuleBase
        where T : ICommandContext
    {
        private T _context;

        /// <summary>
        ///     Gets the command context containing metadata and logging access for the command currently in scope.
        /// </summary>
        public new T Context
        {
            get
                => _context ??= (T)base.Context;
        }
    }

    /// <summary>
    ///     The base type needed to write commands with Commands.NET. This type can be derived freely, in order to extend and implement command functionality. 
    ///     Modules do not have state, they are instantiated and populated before a command runs and immediately disposed when it finishes.
    /// </summary>
    /// <remarks>
    ///      All derived types must be known in <see cref="CommandConfiguration.Assemblies"/> to be discoverable and automatically registered during the creation of a <see cref="CommandManager"/>.
    /// </remarks>
    public abstract class ModuleBase
    {
        /// <summary>
        ///     Gets the command context containing metadata and logging access for the command currently in scope.
        /// </summary>
        public ICommandContext Context { get; internal set; }

        /// <summary>
        ///     Gets the reflection information about this command.
        /// </summary>
        public CommandInfo Command { get; internal set; }

        /// <summary>
        ///     Represents an overridable operation that runs before command invocation starts.
        /// </summary>
        /// <remarks>
        ///     If this method throws an exception, the whole command invocation is halted.
        /// </remarks>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> provided at the root call for this command execution, which can be used to cancel running operations.</param>
        /// <returns>The awaitable result of this asynchronous operation.</returns>
        public virtual ValueTask BeforeExecuteAsync(CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        ///     Represents an overridable operation that runs after command invocation ended.
        /// </summary>
        /// <remarks>
        ///     If command invocation threw an exception, this method will not execute.
        /// </remarks>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> provided at the root call for this command execution, which can be used to cancel running operations.</param>
        /// <returns>The awaitable result of this asynchronous operation.</returns>
        public virtual ValueTask AfterExecuteAsync(CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        ///     Represents an overridable operation that is responsible for resolving unknown invocation results.
        /// </summary>
        /// <param name="value">The invocation result of which no base handler exists.</param>
        /// <returns>The awaitable result of this asynchronous operation.</returns>
        public virtual async ValueTask UnknownReturnAsync(object value)
        {
            await Task.CompletedTask;
        }

        internal async Task<RunResult> ResolveReturnAsync(object value)
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

            return new(Command);
        }
    }
}
