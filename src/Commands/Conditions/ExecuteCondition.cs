﻿namespace Commands.Conditions;

/// <summary>
///     A delegate-based condition that determines whether a command can execute or not.
/// </summary>
/// <typeparam name="TEvaluator">The evaluator type which should wrap this condition.</typeparam>
/// <param name="checkDelegate">The delegate that is triggered when a check is done during command execution to determine if the command can execute or not.</param>
public sealed class DelegateExecuteCondition<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
TEvaluator>(Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>> checkDelegate) : ExecuteCondition<TEvaluator>
    where TEvaluator : ConditionEvaluator, new()
{
    /// <inheritdoc />
    public override ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken)
        => checkDelegate(caller, command, services);
}

/// <inheritdoc />
public abstract class ExecuteCondition<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
# endif
TEvaluator> : ExecuteCondition
    where TEvaluator : ConditionEvaluator, new()
{
    /// <inheritdoc />
#if NET8_0_OR_GREATER
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    public override Type EvaluatorType { get; } = typeof(TEvaluator);
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
    public ConditionResult Error(string error)
    {
        Assert.NotNullOrEmpty(error, nameof(error));

        return ConditionResult.FromError(new ConditionException(this, error));
    }

    /// <inheritdoc />
    public ConditionResult Success()
        => ConditionResult.FromSuccess();

    #region Initializers

    /// <inheritdoc cref="From{T}(Func{ICallerContext, Command, IServiceProvider, ValueTask{ConditionResult}})"/>
    public static ExecuteConditionProperties<T> For<T>()
        where T : ConditionEvaluator, new()
        => new();

    /// <summary>
    ///     Defines a collection of properties to configure and convert into a new instance of <see cref="ExecuteCondition"/>.
    /// </summary>
    /// <param name="executionDelegate">The delegate that should be executed when the condition is invoked.</param>
    /// <returns>A fluent-pattern property object that can be converted into an instance when configured.</returns>
    public static ExecuteConditionProperties<T> From<T>(Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>> executionDelegate)
        where T : ConditionEvaluator, new()
        => new ExecuteConditionProperties<T>().Delegate(executionDelegate);

    #endregion
}