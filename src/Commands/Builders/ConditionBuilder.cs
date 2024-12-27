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
        public Func<ICallerContext, CommandInfo, ConditionTrigger, IServiceProvider, Task<ConditionResult>> Handler { get; set; }

        /// <summary>
        ///     Creates a new instance of <see cref="ConditionBuilder{T}"/> with default values, to be confugured using the fluent API.
        /// </summary>
        public ConditionBuilder()
        {
            Trigger = ConditionTrigger.BeforeInvoke;
            Handler = default!;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ConditionBuilder{T}"/> with the specified trigger and delegate.
        /// </summary>
        /// <param name="trigger">A set of flags which determines when in the execution pipeline the condition should be evaluated.</param>
        /// <param name="func">A delegate that is called when the provided triggers determine that it should be evaluated.</param>
        public ConditionBuilder(ConditionTrigger trigger, Func<ICallerContext, CommandInfo, ConditionTrigger, IServiceProvider, Task<ConditionResult>> func)
        {
            Trigger = trigger;
            Handler = func;
        }

        /// <inheritdoc />
        public IConditionBuilder WithTriggers(ConditionTrigger triggers)
        {
            Trigger = triggers;
            return this;
        }

        /// <inheritdoc />
        public IConditionBuilder WithHandler(Func<ICallerContext, CommandInfo, ConditionTrigger, IServiceProvider, Task<ConditionResult>> func)
        {
            Handler = func;
            return this;
        }

        /// <inheritdoc />
        public IExecuteCondition Build()
        {
            if (Handler is null)
                throw new ArgumentNullException(nameof(Handler));

            return new DelegateCondition<T>(Trigger, Handler);
        }
    }
}
