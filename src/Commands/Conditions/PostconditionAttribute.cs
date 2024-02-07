﻿using Commands.Core;
using Commands.Exceptions;
using Commands.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Commands.Conditions
{
    /// <summary>
    ///     An attribute that defines that a check should succeed before a command can be executed.
    /// </summary>
    /// <remarks>
    ///     The <see cref="EvaluateAsync(IConsumer, ICommandResult, IServiceProvider, CancellationToken)"/> method is responsible for doing this check. 
    ///     Custom implementations of <see cref="PostconditionAttribute"/> can be placed at module or command level, with each being ran in top-down order when a target is checked. 
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class PostconditionAttribute : Attribute
    {
        const string _exHeader = "Postcondition result halted further command execution. View inner exception for more details.";

        /// <summary>
        ///     Evaluates the known data about a command at the point of post-execution, in order to determine if command execution was succesful or not.
        /// </summary>
        /// <remarks>
        ///     Make use of <see cref="Error(Exception)"/> or <see cref="Error(string)"/> and <see cref="Success"/> to safely create the intended result.
        /// </remarks>
        /// <param name="context">Context of the current execution.</param>
        /// <param name="result">The result of the execution.</param>
        /// <param name="services">The provider used to register modules and inject services.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
        public abstract ValueTask<ConditionResult> EvaluateAsync(ConsumerBase context, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken);

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> representing a failed evaluation.
        /// </summary>
        /// <param name="exception">The exception that caused the evaluation to fail.</param>
        /// <returns>A <see cref="ConditionResult"/> representing the failed evaluation.</returns>
        public static ConditionResult Error([DisallowNull] Exception exception)
        {
            if (exception == null)
                ThrowHelpers.ThrowInvalidArgument(exception);

            if (exception is ConditionException checkEx)
            {
                return new(checkEx);
            }
            return new(new ConditionException(_exHeader, exception));
        }

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> representing a failed evaluation.
        /// </summary>
        /// <param name="error">The error that caused the evaluation to fail.</param>
        /// <returns>A <see cref="ConditionResult"/> representing the failed evaluation.</returns>
        public virtual ConditionResult Error([DisallowNull] string error)
        {
            if (string.IsNullOrEmpty(error))
                ThrowHelpers.ThrowInvalidArgument(error);

            return new(new ConditionException(error));
        }

        /// <summary>
        ///     Creates a new <see cref="ConditionResult"/> representing a successful evaluation.
        /// </summary>
        /// <returns>A <see cref="ConditionResult"/> representing the successful evaluation.</returns>
        public virtual ConditionResult Success()
        {
            return new();
        }
    }
}