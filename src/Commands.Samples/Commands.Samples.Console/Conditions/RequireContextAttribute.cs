using Commands.Conditions;

namespace Commands.Samples;

public sealed class RequireContextAttribute<T> : ConditionAttribute<ANDEvaluator>
    where T : ICallerContext
{
    public override ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken)
        => caller is T 
            ? Success() 
            : Error("The command requires a specific context, which is not available.");
}
