namespace Commands.Conditions;

/// <summary>
///     Represents a delegate condition that determines whether a command can execute or not.
/// </summary>
/// <remarks>
///     This condition is bound to a specific <see cref="ICallerContext"/> implementation, and will only evaluate if the provided context is of the expected type.
/// </remarks>
/// <param name="func">A delegate that is executed when the trigger declares that this condition will be evaluated.</param>
public sealed class DelegateCondition<
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

        return ConditionResult.FromError($"The provided {nameof(ICallerContext)} is not of the expected type: {typeof(TContext).Name}.");
    }

#if NET8_0_OR_GREATER
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    Type ICondition.GetEvalType()
        => typeof(TEval);
}
