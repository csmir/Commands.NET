using Commands.Builders;

namespace Commands;

/// <summary>
///     Represents a condition that determines whether a command can execute or not.
/// </summary>
/// <remarks>
///     This condition will only be ran if the provided <see cref="ICallerContext"/> is an instance of <typeparamref name="TContext"/>, otherwise, returning true by default.
/// </remarks>
/// <param name="func">A delegate that is executed when the trigger declares that this condition will be evaluated.</param>
public sealed class CommandCondition<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
# endif
TEval, TContext>(Func<TContext, Command, IServiceProvider, ValueTask<ConditionResult>> func)
    : ICondition
    where TEval : ConditionEvaluator, new()
    where TContext : ICallerContext
{
    /// <inheritdoc />
    public ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (caller is TContext context)
            return func(context, command, services);

        return ConditionResult.FromSuccess();
    }

#if NET8_0_OR_GREATER
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    Type ICondition.GetEvalType()
        => typeof(TEval);
}

/// <summary>
///     Represents a condition that determines whether a command can execute or not.
/// </summary>
public static class CommandCondition
{
    /// <summary>
    ///     Creates a new condition with the provided evaluation method.
    /// </summary>
    /// <typeparam name="TEval">The evaluation approach for this condition.</typeparam>
    /// <param name="evaluation">The evaluation action to determine if command execution can succeed after </param>
    /// <returns>A newly created implementation of <see cref="ICondition"/> that contains an evaluation which will be ran when a command is requested.</returns>
    public static ICondition Create<TEval>(Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>> evaluation)
        where TEval : ConditionEvaluator, new()
        => new CommandCondition<TEval, ICallerContext>(evaluation);

    /// <summary>
    ///     Creates a new condition with the provided evaluation method, which is executed only if the provided <see cref="ICallerContext"/> is an implementation of <typeparamref name="TContext"/>.
    /// </summary>
    /// <typeparam name="TEval">The evaluation approach for this condition.</typeparam>
    /// <typeparam name="TContext">The caller which will filter when this condition is executed.</typeparam>
    /// <param name="evaluation">The evaluation action to determine if command execution can succeed after </param>
    /// <returns>A newly created implementation of <see cref="ICondition"/> that contains an evaluation which will be ran when a command is requested.</returns>
    public static ICondition Create<TEval, TContext>(Func<TContext, Command, IServiceProvider, ValueTask<ConditionResult>> evaluation)
        where TEval : ConditionEvaluator, new()
        where TContext : ICallerContext
        => new CommandCondition<TEval, TContext>(evaluation);

    /// <summary>
    ///     Creates a new <see cref="IConditionBuilder"/> which can be built into a new implementation instance of <see cref="ICondition"/>.
    /// </summary>
    /// <typeparam name="TEval">The evaluation approach for this builder.</typeparam>
    /// <returns>A new instance of <see cref="ConditionBuilder{TEval, TContext}"/> which can be built into a new implementation instance of <see cref="ICondition"/>.</returns>
    public static ConditionBuilder<TEval, ICallerContext> CreateBuilder<TEval>()
        where TEval : ConditionEvaluator, new()
        => new();

    /// <summary>
    ///     Creates a new <see cref="IConditionBuilder"/> which can be built into a new implementation instance of <see cref="ICondition"/>,  which is executed only if the provided <see cref="ICallerContext"/> is an implementation of <typeparamref name="TContext"/>.
    /// </summary>
    /// <typeparam name="TEval">The evaluation approach for this condition.</typeparam>
    /// <typeparam name="TContext">The caller which will filter when this condition is executed.</typeparam>
    /// <returns>A new instance of <see cref="ConditionBuilder{TEval, TContext}"/> which can be built into a new implementation instance of <see cref="ICondition"/>.</returns>
    public static ConditionBuilder<TEval, TContext> CreateBuilder<TEval, TContext>()
        where TEval : ConditionEvaluator, new()
        where TContext : ICallerContext
        => new();
}
