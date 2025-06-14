namespace Commands.Conditions;

/// <summary>
///     An attribute that contains an evaluation method called when marked on top of a command signature.
/// </summary>
/// <remarks>
///     The <see cref="Evaluate(IContext, Command, IServiceProvider, CancellationToken)"/> method is responsible for doing this evaluation. 
///     Custom implementations of <see cref="ExecuteConditionAttribute{T}"/> can be placed at module or command level, with each being ran in top-down order when a target is checked. 
///     If multiple commands are found during matching, multiple sequences of preconditions will be ran to find a match that succeeds.
/// </remarks>
/// <typeparam name="TEvaluator">The type of evaluator that will be used to determine the result of the evaluation.</typeparam>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public abstract class ExecuteConditionAttribute<TEvaluator>() : Attribute, IInternalCondition
    where TEvaluator : ConditionEvaluator, new()
{
    /// <inheritdoc />
    public abstract ValueTask<ConditionResult> Evaluate(
        IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken);

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is <see langword="null"/> or empty.</exception>
    public virtual ConditionResult Error(string error)
    {
        Assert.NotNullOrEmpty(error, nameof(error));

        return ConditionResult.FromError(new ConditionException(this, error));
    }

    /// <inheritdoc />
    public ConditionResult Success()
        => ConditionResult.FromSuccess();

    #region Internals

    /// <inheritdoc />
    string IInternalCondition.EvaluatorName => nameof(TEvaluator);

    ConditionEvaluator IInternalCondition.CreateEvaluator() 
        => new TEvaluator();

    #endregion
}
