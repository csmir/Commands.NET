using Commands.Core.Execution.Abstractions;

namespace Commands.Core.Execution;

/// <inheritdoc cref="ICallerAccessor{T}" />
internal sealed class CallerAccessor<T> : ICallerAccessor<T>
    where T : ICallerContext
{
    internal ICallerContext? ICaller;

    /// <inheritdoc />
    public T Caller
    {
        get
        {
            if (ICaller is T caller)
                return caller;

            throw new InvalidCastException($"The implementation of {nameof(ICallerContext)} for this scope cannot be cast to {typeof(T)}.");
        }
    }
}
