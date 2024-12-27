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
        public ConditionTrigger Triggers { get; set; }

        /// <summary>
        ///     Builds an execution condition from the provided configuration.
        /// </summary>
        /// <returns>A newly created implementation of <see cref="IExecuteCondition"/> representing an evaluation for command execution to succeed or fail.</returns>
        public IExecuteCondition Build();
    }
}
