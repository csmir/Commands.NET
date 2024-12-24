using Commands.Conditions;

namespace Commands.Builders
{
    /// <inheritdoc cref="IConditionBuilder"/>
    public sealed class ConditionBuilder<T> : IConditionBuilder
        where T : ConditionEvaluator, new()
    {
        /// <inheritdoc />
        public ConditionTrigger Trigger { get; set; }

        /// <inheritdoc />
        public Func<ICallerContext, CommandInfo, ConditionTrigger, IServiceProvider, ValueTask<ConditionResult>> Delegate { get; set; }

        /// <summary>
        ///     Creates a new instance of <see cref="ConditionBuilder{T}"/> with default values, to be confugured using the fluent API.
        /// </summary>
        public ConditionBuilder()
        {
            Trigger = ConditionTrigger.BeforeInvoke;
            Delegate = default!;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ConditionBuilder{T}"/> with the specified trigger and delegate.
        /// </summary>
        /// <param name="trigger">A set of flags which determines when in the execution pipeline the condition should be evaluated.</param>
        /// <param name="func">A delegate that is called when the provided triggers determine that it should be evaluated.</param>
        public ConditionBuilder(ConditionTrigger trigger, Func<ICallerContext, CommandInfo, ConditionTrigger, IServiceProvider, ValueTask<ConditionResult>> func)
        {
            Trigger = trigger;
            Delegate = func;
        }

        /// <inheritdoc />
        public IConditionBuilder WithTriggers(ConditionTrigger triggers)
        {
            Trigger = triggers;
            return this;
        }

        /// <inheritdoc />
        public IConditionBuilder WithDelegate(Func<ICallerContext, CommandInfo, ConditionTrigger, IServiceProvider, ValueTask<ConditionResult>> func)
        {
            Delegate = func;
            return this;
        }

        /// <inheritdoc />
        public IExecuteCondition Build()
        {
            if (Delegate is null)
                throw new ArgumentNullException(nameof(Delegate));

            return new DelegateCondition<T>(Trigger, Delegate);
        }
    }
}
