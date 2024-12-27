using System.ComponentModel;

namespace Commands.Conditions
{
    /// <summary>
    ///     Represents a delegate condition that determines whether a command can execute or not.
    /// </summary>
    /// <remarks>
    ///     This condition is bound to a specific <see cref="ICallerContext"/> implementation, and will only evaluate if the provided context is of the expected type.
    /// </remarks>
    /// <param name="trigger">A set of flags that determines when this condition should be evaluated.</param>
    /// <param name="func">A delegate that is executed when the trigger declares that this condition will be evaluated.</param>
    public sealed class DelegateCondition<TEval, TContext>(
        ConditionTrigger trigger, Func<TContext, CommandInfo, ConditionTrigger, IServiceProvider, ValueTask<ConditionResult>> func)
        : IExecuteCondition
        where TEval : ConditionEvaluator, new()
    {
        /// <inheritdoc />
        public ConditionTrigger Trigger { get; } = trigger;

        /// <inheritdoc />
        public ValueTask<ConditionResult> Evaluate(ICallerContext caller, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (caller is TContext context)
                return func(context, command, trigger, services);

            return ConditionResult.FromError($"The provided {nameof(ICallerContext)} is not of the expected type: {typeof(TContext).Name}.");
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConditionEvaluator GetEvaluator()
            => new TEval();

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string GetGroupId()
            => $"{GetType().Name}:{typeof(TEval).Name}:{Trigger}";
    }
}
