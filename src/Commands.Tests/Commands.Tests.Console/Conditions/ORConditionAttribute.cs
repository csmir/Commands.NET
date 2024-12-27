using Commands.Conditions;

namespace Commands.Tests
{
    public class ORConditionAttribute(bool pass) : ConditionAttribute<OREvaluator>
    {
        public override ValueTask<ConditionResult> Evaluate(ICallerContext consumer, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (pass)
                return Success();

            return Error("The condition failed.");
        }
    }
}
