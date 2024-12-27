using Commands.Conditions;
using System.Diagnostics;

namespace Commands
{
    /// <summary>
    ///     The result of a check operation within the command execution pipeline.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public readonly struct ConditionResult : IExecuteResult
    {
        /// <summary>
        ///     Gets during what execution step the condition was triggered.
        /// </summary>
        public ConditionTrigger Trigger { get; }

        /// <inheritdoc />
        public Exception? Exception { get; }

        /// <inheritdoc />
        public bool Success
            => Exception == null;

        private ConditionResult(ConditionTrigger trigger, Exception? exception)
        {
            Trigger = trigger;
            Exception = exception;
        }

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> resembling a successful check operation.
        /// </summary>
        /// <returns>A new result containing information about the succeeded operation.</returns>
        public static ConditionResult FromSuccess()
            => new(ConditionTrigger.None, null);

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> resembling a successful check operation.
        /// </summary>
        /// <param name="trigger">The trigger that caused the condition to be checked.</param>
        /// <returns>A new result containing information about the succeeded operation.</returns>
        public static ConditionResult FromSuccess(ConditionTrigger trigger)
            => new(trigger, null);

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> resembling a failed check operation.
        /// </summary>
        /// <param name="reason">The reason why the check failed.</param>
        /// <returns>A new result containing information about the failed operation.</returns>
        public static ConditionResult FromError(string reason)
            => new(ConditionTrigger.None, new ConditionException(reason));

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> resembling a failed check operation.
        /// </summary>
        /// <param name="exception">The exception that contains the failure reason of this check.</param>
        /// <returns>A new result containing information about the failed operation.</returns>
        public static ConditionResult FromError(Exception exception)
            => new(ConditionTrigger.None, exception);

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> resembling a failed check operation.
        /// </summary>
        /// <param name="trigger">The trigger that caused the condition to be checked.</param>
        /// <param name="exception">The exception that occurred during the check operation.</param>
        /// <returns>A new result containing information about the failed operation.</returns>
        public static ConditionResult FromError(ConditionTrigger trigger, Exception exception)
            => new(trigger, exception);

        /// <inheritdoc />
        public override string ToString()
            => $"Success = {(Exception == null ? "True" : $"False \nException = {Exception.Message}")}";

        /// <summary>
        ///     Gets a string representation of this result.
        /// </summary>
        /// <param name="inline">Sets whether the string representation should be inlined or not.</param>
        /// <returns></returns>
        public string ToString(bool inline)
            => inline ? $"Success = {(Exception == null ? "True" : $"False")}" : ToString();

        /// <summary>
        ///     Implicitly converts a <see cref="ConditionResult"/> to a <see cref="Task{TResult}"/>.
        /// </summary>
        /// <param name="result">The result to convert.</param>
        public static implicit operator Task<ConditionResult>(ConditionResult result)
            => Task.FromResult(result);
    }
}
