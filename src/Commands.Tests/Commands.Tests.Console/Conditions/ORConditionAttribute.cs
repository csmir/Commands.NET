using Commands.Conditions;

namespace Commands.Tests
{
    public class ORConditionAttribute(bool pass) : ConditionAttribute<OREvaluator>
    {
        public override ValueTask<ConditionResult> Evaluate(ICallerContext consumer, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (pass)
                return ValueTask.FromResult(Success());

            return ValueTask.FromResult(Error("The condition failed."));
        }
    }
}
