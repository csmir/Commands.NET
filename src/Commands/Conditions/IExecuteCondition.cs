using Commands.Reflection;
using System.ComponentModel;

namespace Commands.Conditions
{
    /// <summary>
    ///     A condition that is evaluated before or after a command is executed.
    /// </summary>
    public interface IExecuteCondition
    {
        /// <summary>
        ///     Gets the trigger that determines when the condition is evaluated. This enum is flagged, so multiple triggers can be applied by combining them.
        /// </summary>
        public ConditionTrigger Trigger { get; }

        /// <summary>
        ///     Evaluates the known data about a command at this point in execution, in order to determine if command execution can continue or not.
        /// </summary>
        /// <param name="consumer">The consumer of the current execution.</param>
        /// <param name="command">The result of the execution.</param>
        /// <param name="trigger">The trigger that caused the condition to be checked.</param>
        /// <param name="services">The provider used to register modules and inject services.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
        public ValueTask<ConditionResult> Evaluate(CallerContext consumer, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken);

        /// <summary>
        ///     Gets an evaluator that is used to determine the result of the evaluation.
        /// </summary>
        /// <returns>An instance of <see cref="ConditionEvaluator"/> that is capable of evaluating the command.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConditionEvaluator GetEvaluator();

        /// <summary>
        ///     Gets an identifier that represents the group of conditions that are being evaluated, built from the precondition's qualifying type name and the evaluator's type name.
        /// </summary>
        /// <returns>An string identifier representing an item in a group of items.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string GetGroupId();
    }
}
