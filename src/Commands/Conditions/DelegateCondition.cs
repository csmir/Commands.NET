namespace Commands.Conditions;

/// <summary>
///     Represents a delegate condition that determines whether a command can execute or not.
/// </summary>
/// <remarks>
///     This condition will only be ran if the provided <see cref="ICallerContext"/> is an instance of <typeparamref name="TContext"/>, otherwise, returning true by default.
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

        return ConditionResult.FromSuccess();
    }

#if NET8_0_OR_GREATER
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    Type ICondition.GetEvalType()
        => typeof(TEval);
}
