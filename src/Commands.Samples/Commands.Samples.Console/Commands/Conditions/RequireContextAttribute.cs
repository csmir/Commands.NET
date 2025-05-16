using Commands.Conditions;

namespace Commands.Samples;

public sealed class RequireContextAttribute<T> : ExecuteConditionAttribute<ANDEvaluator>
    where T : IContext
{
    public override ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken)
        => context is T
            ? Success()
            : Error("The command requires a specific context, which is not available.");
}
