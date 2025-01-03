using Commands.Conditions;

namespace Commands.Tests;

public class ANDAttribute(bool pass) : ConditionAttribute<ANDEvaluator>
{
    public override ValueTask<ConditionResult> Evaluate(ICallerContext consumer, Commands.Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (pass)
            return Success();

        return Error("The condition failed.");
    }
}
