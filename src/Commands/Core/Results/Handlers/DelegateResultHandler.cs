using System.ComponentModel;

namespace Commands
{
    /// <summary>
    ///     Represents a resolver that invokes a delegate when a result is encountered. This class cannot be inherited.
    /// </summary>
    /// <param name="action">The action to be invoked when receiving a result.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DelegateResultHandler(
        Action<ICallerContext, IExecuteResult, IServiceProvider> action)
        : ResultHandler
    {
        private readonly Action<ICallerContext, IExecuteResult, IServiceProvider> _action = action;

        /// <inheritdoc />
        public override ValueTask HandleResult(
            ICallerContext caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            // If the result is successful and is an invocation result, we can handle it as a successful command.
            if (result.Success && result is InvokeResult invoke)
                return HandleSuccess(caller, invoke, services, cancellationToken);

            _action(caller, result, services);

            return default;
        }
    }
}
