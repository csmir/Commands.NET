using Commands.Conditions;

namespace Commands.Builders
{
    /// <summary>
    ///     A builder model for an execution condition, which is an evaluation that is triggered based on the configured trigger moment, setting up the command to fail or succeed. This class cannot be inherited.
    /// </summary>
    /// <typeparam name="TEval">The evaluation approach that the built condition should have in order to fail or succeed.</typeparam>
    /// <typeparam name="TContext">The context to which the resulting condition is bound, failing automatically if the provided context cannot be cast to it.</typeparam>
    public sealed class ConditionBuilder<TEval, TContext> : IConditionBuilder
        where TEval : ConditionEvaluator, new()
        where TContext : ICallerContext
    {
        /// <inheritdoc />
        public ConditionTrigger Triggers { get; set; }

        /// <inheritdoc />
        public Func<TContext, CommandInfo, ConditionTrigger, IServiceProvider, ValueTask<ConditionResult>> Handler { get; set; }

        /// <summary>
        ///     Creates a new instance of <see cref="ConditionBuilder{T, T}"/> with default values, to be confugured using the fluent API.
        /// </summary>
        public ConditionBuilder()
        {
            Triggers = ConditionTrigger.Execution;
            Handler = default!;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ConditionBuilder{T, T}"/> with the specified trigger and delegate.
        /// </summary>
        /// <param name="trigger">A set of flags which determines when in the execution pipeline the condition should be evaluated.</param>
        /// <param name="executionHandler">A delegate that is called when the provided triggers determine that it should be evaluated.</param>
        public ConditionBuilder(ConditionTrigger trigger, Func<TContext, CommandInfo, ConditionTrigger, IServiceProvider, ValueTask<ConditionResult>> executionHandler)
        {
            Triggers = trigger;
            Handler = executionHandler;
        }

        /// <summary>
        ///     Sets the trigger that represents when the condition should be evaluated during the execution process.
        /// </summary>
        /// <param name="triggers">A set of flags, which can be joined together using bitwise, to determine when the condition should be evaluated during command execution.</param>
        /// <returns>The same <see cref="ConditionBuilder{TEval, TContext}"/> for call-chaining.</returns>
        public ConditionBuilder<TEval, TContext> WithTriggers(ConditionTrigger triggers)
        {
            Triggers = triggers;
            return this;
        }

        /// <summary>
        ///     Sets the delegate that is executed when the trigger declares that this condition will be evaluated.
        /// </summary>
        /// <param name="executionHandler">A delegate that contains logic to be executed when called by the execution pipeline.</param>
        /// <returns>The same <see cref="ConditionBuilder{TEval, TContext}"/> for call-chaining.</returns>
        public ConditionBuilder<TEval, TContext> WithHandler(Func<TContext, CommandInfo, ConditionTrigger, IServiceProvider, ValueTask<ConditionResult>> executionHandler)
        {
            Handler = executionHandler;
            return this;
        }

        /// <inheritdoc />
        public ICondition Build()
        {
            Assert.NotNull(Handler, nameof(Handler));

            return new DelegateCondition<TEval, TContext>(Triggers, Handler);
        }
    }
}
