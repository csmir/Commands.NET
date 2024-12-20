using System.ComponentModel;

namespace Commands
{    
    /// <summary>
    ///     Represents a resolver that invokes a delegate when a result is encountered from a command implementing <typeparamref name="T"/>. This class cannot be inherited.
    /// </summary>
    /// <param name="func">The action to be invoked when receiving a result.</param>
    public sealed class AsyncDelegateResultHandler<T>(
        Func<T, IExecuteResult, IServiceProvider, ValueTask> func)
        : ResultHandler<T>
        where T : class, ICallerContext
    {
        /// <inheritdoc />
        public override ValueTask HandleResult(T caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (result.Success && result is InvokeResult invoke)
                return HandleSuccess(caller, invoke, services, cancellationToken);

            return func(caller, result, services);
        }
    }

    /// <summary>
    ///     Represents a resolver that invokes a delegate when a result is encountered. This class cannot be inherited.
    /// </summary>
    /// <param name="action">The action to be invoked when receiving a result.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class AsyncDelegateResultHandler(
        Func<ICallerContext, IExecuteResult, IServiceProvider, ValueTask> action)
        : ResultHandler
    {
        private readonly Func<ICallerContext, IExecuteResult, IServiceProvider, ValueTask> _action = action;

        /// <inheritdoc />
        public override async ValueTask HandleResult(
            ICallerContext caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            // If the result is successful and is an invocation result, we can handle it as a successful command.
            if (result.Success && result is InvokeResult invoke)
                await HandleSuccess(caller, invoke, services, cancellationToken);

            await _action(caller, result, services);
        }
    }
}
