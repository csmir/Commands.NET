using System.ComponentModel;

namespace Commands;

/// <summary>
///     Represents a delegate-based command condition.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DelegateExecuteCondition<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
TEval>(Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>> func) : ExecuteCondition<TEval>
    where TEval : ConditionEvaluator, new()
{
    /// <inheritdoc />
    public override ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken)
        => func(caller, command, services);
}

/// <inheritdoc />
public abstract class ExecuteCondition<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
# endif
TEval> : ExecuteCondition
    where TEval : ConditionEvaluator, new()
{
    /// <inheritdoc />
#if NET8_0_OR_GREATER
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    public override Type EvaluatorType { get; } = typeof(TEval);
}

/// <summary>
///     Represents a condition that determines whether a command can execute or not.
/// </summary>
public abstract class ExecuteCondition : IExecuteCondition
{
    /// <inheritdoc />
#if NET8_0_OR_GREATER
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    public abstract Type EvaluatorType { get; }

    /// <inheritdoc />
    public abstract ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken);

    /// <inheritdoc />
    public ConditionResult Error(Exception exception)
    {
        Assert.NotNull(exception, nameof(exception));

        if (exception is ConditionException conEx)
            return ConditionResult.FromError(conEx);

        return ConditionResult.FromError(ConditionException.ConditionFailed(exception));
    }

    /// <inheritdoc />
    public ConditionResult Error(string error)
    {
        Assert.NotNullOrEmpty(error, nameof(error));

        return ConditionResult.FromError(new ConditionException(error));
    }

    /// <inheritdoc />
    public ConditionResult Success()
        => ConditionResult.FromSuccess();
}