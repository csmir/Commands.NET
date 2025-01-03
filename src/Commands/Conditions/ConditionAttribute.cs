namespace Commands.Conditions;

/// <summary>
///     An attribute that contains an evaluation method called when marked on top of a command signature, implementing <see cref="ConditionAttribute{T}"/>. This attribute will only be ran if the provided <see cref="ICallerContext"/> is an instance of <typeparamref name="TContext"/>, otherwise, returning true by default.
/// </summary>
/// <remarks>
///     Custom implementations of <see cref="ConditionAttribute{T, T}"/> can be placed at module or command level, with each being ran in top-down order when a target is checked. 
/// </remarks>
/// <typeparam name="TEval">The type of evaluator that will be used to determine the result of the evaluation.</typeparam>
/// <typeparam name="TContext">The implementation of <see cref="ICallerContext"/> that this operation should match in order to validate the condition.</typeparam>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public abstract class ConditionAttribute<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
TEval, TContext> : ConditionAttribute<TEval>
    where TEval : ConditionEvaluator, new()
    where TContext : ICallerContext
{
    /// <inheritdoc />
    public override ValueTask<ConditionResult> Evaluate(
        ICallerContext context, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (context is TContext caller)
            return Evaluate(caller, command, services, cancellationToken);

        return Success();
    }

    /// <inheritdoc cref="Evaluate(ICallerContext, Command, IServiceProvider, CancellationToken)" />
    /// <remarks>
    ///     Evaluates the condition for the given context, command, trigger, services and cancellation token. This evaluation only succeeds if the provided <see cref="ICallerContext"/> is an instance of <typeparamref name="TContext"/>.
    /// </remarks>
    public abstract ValueTask<ConditionResult> Evaluate(
        TContext context, Command command, IServiceProvider services, CancellationToken cancellationToken);
}

/// <summary>
///     An attribute that contains an evaluation method called when marked on top of a command signature.
/// </summary>
/// <remarks>
///     The <see cref="Evaluate(ICallerContext, Command, IServiceProvider, CancellationToken)"/> method is responsible for doing this evaluation. 
///     Custom implementations of <see cref="ConditionAttribute{T}"/> can be placed at module or command level, with each being ran in top-down order when a target is checked. 
///     If multiple commands are found during matching, multiple sequences of preconditions will be ran to find a match that succeeds.
/// </remarks>
/// <typeparam name="TEval">The type of evaluator that will be used to determine the result of the evaluation.</typeparam>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public abstract class ConditionAttribute<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
# endif
TEval>() : Attribute, ICondition
    where TEval : ConditionEvaluator, new()
{
    /// <inheritdoc />
    /// <remarks>
    ///     Make use of <see cref="Error(Exception)"/> or <see cref="Error(string)"/> and <see cref="Success"/> to safely create the intended result.
    /// </remarks>
    public abstract ValueTask<ConditionResult> Evaluate(
        ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken);

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

#if NET8_0_OR_GREATER
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    Type ICondition.GetEvalType()
        => typeof(TEval);
}
