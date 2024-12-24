using System.ComponentModel;

namespace Commands.Conditions
{
    /// <summary>
    ///     An attribute that contains an evaluation method called when marked on top of a command signature, implementing <see cref="ConditionAttribute{T}"/> with an <see cref="ANDEvaluator"/>. 
    ///     For use of other evaluators, use <see cref="ConditionAttribute{T}"/>.
    /// </summary>
    /// <remarks>
    ///     Custom implementations of <see cref="ConditionAttribute"/> can be placed at module or command level, with each being ran in top-down order when a target is checked. 
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ConditionAttribute : ConditionAttribute<ANDEvaluator>
    {

    }

    /// <summary>
    ///     An attribute that contains an evaluation method called when marked on top of a command signature.
    /// </summary>
    /// <remarks>
    ///     The <see cref="Evaluate(ICallerContext, CommandInfo, ConditionTrigger, IServiceProvider, CancellationToken)"/> method is responsible for doing this evaluation. 
    ///     Custom implementations of <see cref="ConditionAttribute{T}"/> can be placed at module or command level, with each being ran in top-down order when a target is checked. 
    ///     If multiple commands are found during matching, multiple sequences of preconditions will be ran to find a match that succeeds.
    /// </remarks>
    /// <typeparam name="T">The type of evaluator that will be used to determine the result of the evaluation.</typeparam>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ConditionAttribute<T>() : Attribute, IExecuteCondition
        where T : ConditionEvaluator, new()
    {
        /// <inheritdoc />
        public virtual ConditionTrigger Trigger { get; } = ConditionTrigger.BeforeInvoke;

        /// <inheritdoc />
        /// <remarks>
        ///     Make use of <see cref="Error(Exception)"/> or <see cref="Error(string)"/> and <see cref="Success"/> to safely create the intended result.
        /// </remarks>
        public abstract ValueTask<ConditionResult> Evaluate(
            ICallerContext caller, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConditionEvaluator GetEvaluator()
            => new T() { Trigger = Trigger };

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual string GetGroupId()
            => $"{GetType().Name}:{typeof(T).Name}:{Trigger}";

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> representing a failed evaluation.
        /// </summary>
        /// <param name="exception">The exception that caused the evaluation to fail.</param>
        /// <returns>A <see cref="ConditionResult"/> representing the failed evaluation.</returns>
        protected ConditionResult Error(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (exception is ConditionException checkEx)
                return ConditionResult.FromError(Trigger, checkEx);

            return ConditionResult.FromError(Trigger, ConditionException.ConditionFailed(exception));
        }

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> representing a failed evaluation.
        /// </summary>
        /// <param name="error">The error that caused the evaluation to fail.</param>
        /// <returns>A <see cref="ConditionResult"/> representing the failed evaluation.</returns>
        protected ConditionResult Error(string error)
        {
            if (string.IsNullOrEmpty(error))
                throw new ArgumentNullException(nameof(error));

            return ConditionResult.FromError(Trigger, new ConditionException(error));
        }

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> representing a successful evaluation.
        /// </summary>
        /// <returns>A <see cref="ConditionResult"/> representing the successful evaluation.</returns>
        protected ConditionResult Success()
            => ConditionResult.FromSuccess(Trigger);
    }
}
