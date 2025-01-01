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
    ///     An attribute that contains an evaluation method called when marked on top of a command signature, implementing <see cref="ConditionAttribute{T}"/> and adding validation the implementing <see cref="ICallerContext"/> to be an instance of <typeparamref name="TContext"/>.
    /// </summary>
    /// <remarks>
    ///     Custom implementations of <see cref="ConditionAttribute{T, T}"/> can be placed at module or command level, with each being ran in top-down order when a target is checked. 
    /// </remarks>
    /// <typeparam name="TEval">The type of evaluator that will be used to determine the result of the evaluation.</typeparam>
    /// <typeparam name="TContext">The implementation of <see cref="ICallerContext"/> that this operation should match in order to validate the condition.</typeparam>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ConditionAttribute<TEval, TContext> : ConditionAttribute<TEval>
        where TEval : ConditionEvaluator, new()
        where TContext : ICallerContext
    {
        /// <inheritdoc />
        public override ValueTask<ConditionResult> Evaluate(
            ICallerContext context, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (context is TContext caller)
                return Evaluate(caller, command, trigger, services, cancellationToken);

            return Error($"The provided {nameof(ICallerContext)} is not of the expected type: {typeof(TContext).Name}.");
        }

        /// <inheritdoc cref="Evaluate(ICallerContext, CommandInfo, ConditionTrigger, IServiceProvider, CancellationToken)" />
        /// <remarks>
        ///     Evaluates the condition for the given context, command, trigger, services and cancellation token. This evaluation only succeeds if the provided <see cref="ICallerContext"/> is an instance of <typeparamref name="TContext"/>.
        /// </remarks>
        public abstract ValueTask<ConditionResult> Evaluate(
            TContext context, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken);
    }

    /// <summary>
    ///     An attribute that contains an evaluation method called when marked on top of a command signature.
    /// </summary>
    /// <remarks>
    ///     The <see cref="Evaluate(ICallerContext, CommandInfo, ConditionTrigger, IServiceProvider, CancellationToken)"/> method is responsible for doing this evaluation. 
    ///     Custom implementations of <see cref="ConditionAttribute{T}"/> can be placed at module or command level, with each being ran in top-down order when a target is checked. 
    ///     If multiple commands are found during matching, multiple sequences of preconditions will be ran to find a match that succeeds.
    /// </remarks>
    /// <typeparam name="TEval">The type of evaluator that will be used to determine the result of the evaluation.</typeparam>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ConditionAttribute<TEval>() : Attribute, ICondition
        where TEval : ConditionEvaluator, new()
    {
        /// <inheritdoc />
        public virtual ConditionTrigger Triggers { get; } = ConditionTrigger.Execution;

        /// <inheritdoc />
        /// <remarks>
        ///     Make use of <see cref="Error(Exception)"/> or <see cref="Error(string)"/> and <see cref="Success"/> to safely create the intended result.
        /// </remarks>
        public abstract ValueTask<ConditionResult> Evaluate(
            ICallerContext caller, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConditionEvaluator GetEvaluator()
            => new TEval() { Trigger = Triggers };

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual string GetGroupId()
            => $"{GetType().Name}:{typeof(TEval).Name}:{Triggers}";

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> representing a failed evaluation.
        /// </summary>
        /// <param name="exception">The exception that caused the evaluation to fail.</param>
        /// <returns>A <see cref="ConditionResult"/> representing the failed evaluation.</returns>
        protected ConditionResult Error(Exception exception)
        {
            Assert.NotNull(exception, nameof(exception));

            if (exception is ConditionException conEx)
                return ConditionResult.FromError(conEx);

            return ConditionResult.FromError(ConditionException.ConditionFailed(exception));
        }

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> representing a failed evaluation.
        /// </summary>
        /// <param name="error">The error that caused the evaluation to fail.</param>
        /// <returns>A <see cref="ConditionResult"/> representing the failed evaluation.</returns>
        protected ConditionResult Error(string error)
        {
            Assert.NotNullOrEmpty(error, nameof(error));

            return ConditionResult.FromError(new ConditionException(error));
        }

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> representing a successful evaluation.
        /// </summary>
        /// <returns>A <see cref="ConditionResult"/> representing the successful evaluation.</returns>
        protected ConditionResult Success()
            => ConditionResult.FromSuccess();
    }
}
