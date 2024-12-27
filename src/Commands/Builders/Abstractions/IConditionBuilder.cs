using Commands.Conditions;

namespace Commands.Builders
{
    /// <summary>
    ///     Represents a builder for an execution condition, which is a condition that is evaluated before the command is executed.
    /// </summary>
    public interface IConditionBuilder
    {
        /// <summary>
        ///     Gets or sets the trigger that represents when the condition should be evaluated during the execution process.
        /// </summary>
        public ConditionTrigger Trigger { get; set; }

        /// <summary>
        ///     Gets or sets the condition that is evaluated before the command is executed.
        /// </summary>
        public Func<ICallerContext, CommandInfo, ConditionTrigger, IServiceProvider, Task<ConditionResult>> Handler { get; set; }

        /// <summary>
        ///     Sets the trigger that represents when the condition should be evaluated during the execution process.
        /// </summary>
        /// <param name="triggers">A set of flags, which can be joined together using bitwise, to determine when the condition should be evaluated during command execution.</param>
        /// <returns>The same <see cref="IConditionBuilder"/> for call-chaining.</returns>
        public IConditionBuilder WithTriggers(ConditionTrigger triggers);

        /// <summary>
        ///     Sets the delegate that is executed when the trigger declares that this condition will be evaluated.
        /// </summary>
        /// <param name="executionHandler">A delegate that contains logic to be executed when called by the execution pipeline.</param>
        /// <returns>The same <see cref="IConditionBuilder"/> for call-chaining.</returns>
        public IConditionBuilder WithHandler(Func<ICallerContext, CommandInfo, ConditionTrigger, IServiceProvider, Task<ConditionResult>> executionHandler);

        /// <summary>
        ///     Builds an execution condition from the provided configuration.
        /// </summary>
        /// <returns>A newly created implementation of <see cref="IExecuteCondition"/> representing an evaluation for command execution to succeed or fail.</returns>
        public IExecuteCondition Build();
    }
}
