using System.ComponentModel;

namespace Commands.Conditions
{
    /// <summary>
    ///     Represents a delegate condition that determines whether a command can execute or not.
    /// </summary>
    /// <param name="trigger">A set of flags that determines when this condition should be evaluated.</param>
    /// <param name="func">A delegate that is executed when the trigger declares that this condition will be evaluated.</param>
    public sealed class DelegateCondition<T>(
        ConditionTrigger trigger, Func<ICallerContext, CommandInfo, ConditionTrigger, IServiceProvider, ValueTask<ConditionResult>> func)
        : IExecuteCondition
        where T : ConditionEvaluator, new()
    {
        /// <inheritdoc />
        public ConditionTrigger Trigger { get; } = trigger;

        /// <inheritdoc />
        public ValueTask<ConditionResult> Evaluate(ICallerContext caller, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken)
            => func(caller, command, trigger, services);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConditionEvaluator GetEvaluator()
            => new T();

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string GetGroupId()
            => $"{GetType().Name}:{typeof(T).Name}:{Trigger}";
    }
}
