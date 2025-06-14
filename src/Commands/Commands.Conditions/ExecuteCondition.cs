namespace Commands.Conditions;

/// <summary>
///     A delegate-based condition that determines whether a command can execute or not.
/// </summary>
/// <typeparam name="TEvaluator">The evaluator type which should wrap this condition.</typeparam>
/// <param name="checkDelegate">The delegate that is triggered when a check is done during command execution to determine if the command can execute or not.</param>
public sealed class ConditionDelegate<TEvaluator>(Func<IContext, Command, IServiceProvider, ValueTask<ConditionResult>> checkDelegate) : ExecuteCondition<TEvaluator>
    where TEvaluator : ConditionEvaluator, new()
{
    /// <inheritdoc />
    public override ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken)
        => checkDelegate(context, command, services);
}

/// <inheritdoc />
public abstract class ExecuteCondition<TEvaluator> : IInternalCondition
    where TEvaluator : ConditionEvaluator, new()
{
    /// <inheritdoc />
    public abstract ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken);

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is <see langword="null"/> or empty.</exception>
    public ConditionResult Error(string error)
    {
        Assert.NotNullOrEmpty(error, nameof(error));

        return ConditionResult.FromError(new ConditionException(this, error));
    }

    /// <inheritdoc />
    public ConditionResult Success()
        => ConditionResult.FromSuccess();

    #region Internals

    string IInternalCondition.EvaluatorName => nameof(TEvaluator);

    ConditionEvaluator IInternalCondition.CreateEvaluator()
        => new TEvaluator();

    #endregion
}