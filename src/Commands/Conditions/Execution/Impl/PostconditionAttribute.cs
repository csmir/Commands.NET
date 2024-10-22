using Commands.Exceptions;
using Commands.Helpers;
using Commands.Reflection;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Conditions
{
    /// <summary>
    ///     An attribute that defines that a check should succeed after a command can be executed, implementing <see cref="PostconditionAttribute{T}"/> with an <see cref="ANDEvaluator"/>. 
    ///     For use of other evaluators, use <see cref="PostconditionAttribute{T}"/>.
    /// </summary>
    /// <remarks>
    ///     Custom implementations of <see cref="PostconditionAttribute"/> can be placed at module or command level, with each being ran in top-down order when a target is checked. 
    /// </remarks>
    public abstract class PostconditionAttribute : PostconditionAttribute<ANDEvaluator>
    {

    }

    /// <summary>
    ///     An attribute that defines that a check should succeed after a command can be executed.
    /// </summary>
    /// <remarks>
    ///     The <see cref="Evaluate(ConsumerBase, CommandInfo, IServiceProvider, CancellationToken)"/> method is responsible for doing this check. 
    ///     Custom implementations of <see cref="PostconditionAttribute{T}"/> can be placed at module or command level, with each being ran in top-down order when a target is checked. 
    /// </remarks>
    /// <typeparam name="T">The type of evaluator that will be used to determine the result of the evaluation.</typeparam>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class PostconditionAttribute<T> : Attribute, IPostExecutionCondition
        where T : ConditionEvaluator, new()
    {
        /// <inheritdoc />
        /// <remarks>
        ///     Make use of <see cref="Error(Exception)"/> or <see cref="Error(string)"/> and <see cref="Success"/> to safely create the intended result.
        /// </remarks>
        public abstract ValueTask<ConditionResult> Evaluate(
            ConsumerBase consumer, CommandInfo command, IServiceProvider services, CancellationToken cancellationToken);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual ConditionEvaluator GetEvaluator()
        {
            return new T();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual string GetGroupId()
        {
            return $"{GetType().Name}:{typeof(T).Name}";
        }

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> representing a failed evaluation.
        /// </summary>
        /// <param name="exception">The exception that caused the evaluation to fail.</param>
        /// <returns>A <see cref="ConditionResult"/> representing the failed evaluation.</returns>
        protected ConditionResult Error([DisallowNull] Exception exception)
        {
            if (exception == null)
                ThrowHelpers.ThrowInvalidArgument(exception);

            if (exception is ConditionException checkEx)
            {
                return ConditionResult.FromError(checkEx);
            }

            return ConditionResult.FromError(ConditionException.PostconditionFailed(exception));
        }

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> representing a failed evaluation.
        /// </summary>
        /// <param name="error">The error that caused the evaluation to fail.</param>
        /// <returns>A <see cref="ConditionResult"/> representing the failed evaluation.</returns>
        protected ConditionResult Error([DisallowNull] string error)
        {
            if (string.IsNullOrEmpty(error))
                ThrowHelpers.ThrowInvalidArgument(error);

            return ConditionResult.FromError(new ConditionException(error));
        }

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> representing a successful evaluation.
        /// </summary>
        /// <returns>A <see cref="ConditionResult"/> representing the successful evaluation.</returns>
        protected ConditionResult Success()
        {
            return ConditionResult.FromSuccess();
        }
    }
}
